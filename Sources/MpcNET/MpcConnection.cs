// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpcConnection.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using MpcNET.Commands;
    using MpcNET.Exceptions;
    using MpcNET.Message;
    using Sundew.Base.ControlFlow;

    /// <summary>
    /// Keeps the connection to the MPD server and handels the most basic structure of the MPD protocol.
    /// class.
    /// </summary>
    public class MpcConnection : IMpcConnection
    {
        private readonly Encoding Encoding = new UTF8Encoding();
        private readonly IMpcConnectionReporter mpcConnectionReporter;
        private readonly IPEndPoint server;

        private TcpClient tcpClient;
        private NetworkStream networkStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpcConnection" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="mpcConnectionReporter">The MPC connection logger.</param>
        public MpcConnection(IPEndPoint server, IMpcConnectionReporter mpcConnectionReporter = null)
        {
            this.mpcConnectionReporter = mpcConnectionReporter;
            this.ClearConnectionFields();
            this.server = server ?? throw new ArgumentNullException("Server IPEndPoint not set.", nameof(server));
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Event emitted when the connection is cut.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Connects asynchronously.
        /// </summary>
        /// <returns>The connect task.</returns>
        public async Task ConnectAsync()
        {
            if (this.tcpClient != null)
            {
                var pingResult = await this.PingAsync();
                if (pingResult)
                {
                    return;
                }
            }

            await this.ReconnectAsync(false);
        }

        /// <summary>
        /// Disconnects asynchronously.
        /// </summary>
        /// <returns>The disconnect task.</returns>
        public Task DisconnectAsync()
        {
            return this.DisconnectAsync(true);
        }

        /// <summary>
        /// Sends the command asynchronously.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="mpcCommand">The MPC command.</param>
        /// <returns>
        /// The send task.
        /// </returns>
        public async Task<IMpdMessage<TResponse>> SendAsync<TResponse>(IMpcCommand<TResponse> mpcCommand)
        {
            if (this.tcpClient == null)
            {
                await this.ReconnectAsync(true);
            }

            if (mpcCommand == null)
            {
                throw new CommandNullException();
            }

            Exception lastException = null;
            
            IReadOnlyList<string> response = new List<string>();
            byte[] rawResponse = null;

            var sendAttempter = new Attempter(3);
            var commandText = mpcCommand.Serialize();
            this.mpcConnectionReporter?.Sending(commandText);
            while (sendAttempter.Attempt())
            {
                try
                {
                    using (var writer = new StreamWriter(this.networkStream, Encoding, 512, true) { NewLine = "\n" })
                    {
                        await writer.WriteLineAsync(commandText);
                        await writer.FlushAsync();
                    }

                    (rawResponse, response) = this.ReadResponse(commandText);
                    if (response.Any())
                    {
                        lastException = null;
                        break;
                    }

                    throw new EmptyResponseException(commandText);
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    this.mpcConnectionReporter?.SendException(commandText, sendAttempter.CurrentAttempt, exception);
                    await this.ReconnectAsync(true);
                    this.mpcConnectionReporter?.RetrySend(commandText, sendAttempter.CurrentAttempt);
                }
            }

            if (lastException != null)
            {
                try
                {
                    await this.DisconnectAsync(false);
                }
                catch (Exception)
                {
                }

                return new ErrorMpdMessage<TResponse>(mpcCommand, new ErrorMpdResponse<TResponse>(lastException));
            }

            return new MpdMessage<TResponse>(mpcCommand, true, response, rawResponse);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.DisconnectAsync().Wait();
        }

        private async Task<bool> PingAsync()
        {
            try
            {
                using (var writer = new StreamWriter(this.networkStream, Encoding, 512, true) { NewLine = "\n" })
                {
                    await writer.WriteLineAsync("ping");
                    await writer.FlushAsync();
                }

                using (var reader = new StreamReader(this.networkStream, Encoding, true, 512, true))
                {
                    var responseLine = await reader.ReadLineAsync();
                    return responseLine == "OK";
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ReconnectAsync(bool isReconnect)
        {
            var connectAttempter = new Attempter(3);
            while (connectAttempter.Attempt())
            {
                this.mpcConnectionReporter?.Connecting(isReconnect, connectAttempter.CurrentAttempt);
                await this.DisconnectAsync(false);

                this.tcpClient = new TcpClient();
                await this.tcpClient.ConnectAsync(this.server.Address, this.server.Port);
                if (this.tcpClient.Connected)
                {
                    this.mpcConnectionReporter?.ConnectionAccepted(isReconnect, connectAttempter.CurrentAttempt);
                    break;
                }
            }

            this.networkStream = this.tcpClient.GetStream();
            using (var reader = new StreamReader(this.networkStream, Encoding, true, 512, true))
            {
                var firstLine = await reader.ReadLineAsync();
                if (firstLine != null && !firstLine.StartsWith(Constants.FirstLinePrefix))
                {
                    await this.DisconnectAsync(false);
                    throw new MpcConnectException("Response of mpd does not start with \"" + Constants.FirstLinePrefix + "\".");
                }

                this.Version = firstLine?.Substring(Constants.FirstLinePrefix.Length);
                this.mpcConnectionReporter?.Connected(isReconnect, connectAttempter.CurrentAttempt, firstLine);
            }
        }

        private Tuple<byte[],IReadOnlyList<string>> ReadResponse(string commandText)
        {
            var response = new List<string>();
            byte[] binaryResponse = null;

            var reader = new MpdResponseReader(this.networkStream, Encoding);
            MpdResponseReader.NextData nextData;

            string responseLine;
            while ((nextData = reader.ReportNextData()) != MpdResponseReader.NextData.Eof)
            {
                // If the incoming data is binary, read it raw
                if (nextData == MpdResponseReader.NextData.BinaryData)
                {
                    // The reader already knows the length of the binary data, so we just tell it to read.
                    // MPD binary responses usually don't go past 8192 bytes.
                    byte[] buf = new byte[8192];

                    using (MemoryStream ms = new MemoryStream())
                    {
                        do
                        {
                            var bytesRead = reader.ReadBinaryData(buf, 0, buf.Length);
                            ms.Write(buf, 0, bytesRead);
                        } while (reader.ReportNextData() == MpdResponseReader.NextData.BinaryData);

                        binaryResponse = ms.ToArray();
                    }
                }
                else // else, read string as usual
                {
                    responseLine = reader.ReadString();

                    this.mpcConnectionReporter?.ReadResponse(responseLine, commandText);
                    if (responseLine == null)
                    {
                        break;
                    }

                    response.Add(responseLine);

                    if (responseLine.Equals(Constants.Ok) || responseLine.StartsWith(Constants.Ack))
                    {
                        // Stop reading the stream
                        break;
                    }

                }

            }

            return new Tuple<byte[], IReadOnlyList<string>>(binaryResponse, response);
        }

        private Task DisconnectAsync(bool isExplicitDisconnect)
        {
            if (this.tcpClient == null)
            {
                return Task.CompletedTask;
            }

            this.mpcConnectionReporter?.Disconnecting(isExplicitDisconnect);
            this.ClearConnectionFields();
            this.mpcConnectionReporter?.Disconnected(isExplicitDisconnect);

            Disconnected?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        private void ClearConnectionFields()
        {
            this.networkStream?.Dispose();
            this.tcpClient?.Dispose();
            this.Version = string.Empty;
            this.tcpClient = null;
            this.networkStream = null;
        }
    }
}
