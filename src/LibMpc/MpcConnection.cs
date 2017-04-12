using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LibMpc
{
    /// <summary>
    /// Keeps the connection to the MPD server and handels the most basic structure of the
    /// MPD protocol. The high level commands are handeled in the <see cref="Mpc"/>
    /// class.
    /// </summary>
    public class MpcConnection
    {
        private readonly IPEndPoint _server;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private StreamReader _reader;
        private StreamWriter _writer;

        private string _version;

        public MpcConnection(IPEndPoint server)
        {
            if (IsConnected) return;

            ClearConnectionFields();
            _server = server;
        }

        public bool IsConnected => (_tcpClient != null) && _tcpClient.Connected;
        public string Version => _version;

        public async Task ConnectAsync()
        {
            if (_server == null)
                throw new InvalidOperationException("Server IPEndPoint not set.");

            if (IsConnected)
                throw new AlreadyConnectedException();


            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_server.Address, _server.Port);

            _networkStream = _tcpClient.GetStream();

            // Encoding UTF8 has some problems with TcpClient: https://bugs.musicpd.org/view.php?id=4501
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream) { NewLine = "\n" };

            var firstLine = _reader.ReadLine();
            if (!firstLine.StartsWith(Constants.FirstLinePrefix))
            {
                await DisconnectAsync();
                throw new InvalidDataException("Response of mpd does not start with \"" + Constants.FirstLinePrefix + "\"." );
            }
            _version = firstLine.Substring(Constants.FirstLinePrefix.Length);
        }
        
        public Task DisconnectAsync()
        {
            if (_tcpClient == null)
            {
                return Task.CompletedTask;
            }

            _networkStream.Dispose();
            ClearConnectionFields();

            return Task.CompletedTask;
        }
        

        public async Task<IMpdMessage<T>> SendAsync<T>(IMpcCommand<T> command)
        {
            command.CheckNotNull();

            var connected = await CheckConnectionAsync();
            string[] response;

            try
            {
                _writer.WriteLine(command.Value);
                _writer.Flush();

                response = await ReadResponseAsync();
            }
            catch (Exception)
            {
                try { await DisconnectAsync(); } catch (Exception) { }
                return null; // TODO: Create Null Object for MpdResponse
            }

            return new MpdMessage<T>(command, connected, response);
        }

        private async Task<bool> CheckConnectionAsync()
        {
            if (!IsConnected)
            {
                await ConnectAsync();
            }

            return IsConnected;
        }

        private async Task<string[]> ReadResponseAsync()
        {
            var response = new List<string>();

            // Read response untli reach end token (OK or ACK)
            string responseLine;
            do
            {
                responseLine = await _reader.ReadLineAsync();
                response.Add(responseLine);
            } while (!(responseLine.Equals(Constants.Ok) || responseLine.StartsWith(Constants.Ack)));

            return response.Where(line => !string.IsNullOrEmpty(line)).ToArray();
        }

        private void ClearConnectionFields() 
        {
            _writer?.Dispose();
            _reader?.Dispose();
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
            _version = string.Empty;
        }
    }
}
