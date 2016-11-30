using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly string FIRST_LINE_PREFIX = "OK MPD ";

        private static readonly string OK = "OK";
        private static readonly string ACK = "ACK";

        private static readonly Regex ACK_REGEX = new Regex("^ACK \\[(?<code>[0-9]*)@(?<nr>[0-9]*)] \\{(?<command>[a-z]*)} (?<message>.*)$");

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

            _reader = new StreamReader(_networkStream, Encoding.UTF8);
            _writer = new StreamWriter(_networkStream, Encoding.UTF8) { NewLine = "\n" };

            var firstLine = _reader.ReadLine();
            if (!firstLine.StartsWith(FIRST_LINE_PREFIX))
            {
                await Disconnect();
                throw new InvalidDataException("Response of mpd does not start with \"" + FIRST_LINE_PREFIX + "\"." );
            }
            _version = firstLine.Substring(FIRST_LINE_PREFIX.Length);

            await _writer.WriteLineAsync();
            _writer.Flush();

            ReadResponse();
        }
        /// <summary>
        /// Disconnects from the current MPD server.
        /// </summary>
        private Task Disconnect()
        {
            if (_tcpClient == null)
            {
                return Task.CompletedTask;
            }

            _networkStream.Dispose();
            ClearConnectionFields();

            return Task.CompletedTask;
        }
        /// <summary>
        /// Executes a simple command without arguments on the MPD server and returns the response.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The MPD server response parsed into a basic object.</returns>
        /// <exception cref="ArgumentException">If the command contains a space of a newline charakter.</exception>
        public async Task<MpdResponse> Exec(string command)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (command.Contains(" "))
                throw new ArgumentException("command contains space");
            if (command.Contains("\n"))
                throw new ArgumentException("command contains newline");

            // TODO: Integrate connection status in MpdResponse
            var connectionResult = await CheckConnectionAsync();

            try
            {
                _writer.WriteLine(command);
                _writer.Flush();

                return ReadResponse();
            }
            catch (Exception)
            {
                try { await Disconnect(); } catch (Exception) { }
                return null; // TODO: Create Null Object for MpdResponse
            }
        }
        /// <summary>
        /// Executes a MPD command with arguments on the MPD server.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="argument">The arguments of the command.</param>
        /// <returns>The MPD server response parsed into a basic object.</returns>
        /// <exception cref="ArgumentException">If the command contains a space of a newline charakter.</exception>
        public async Task<MpdResponse> Exec(string command, string[] argument)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (command.Contains(" "))
                throw new ArgumentException("command contains space");
            if (command.Contains("\n"))
                throw new ArgumentException("command contains newline");

            if (argument == null)
                throw new ArgumentNullException("argument");
            for (int i = 0; i < argument.Length; i++)
            {
                if (argument[i] == null)
                    throw new ArgumentNullException("argument[" + i + "]");
                if (argument[i].Contains("\n"))
                    throw new ArgumentException("argument[" + i + "] contains newline");
            }

            // TODO: Integrate connection status in MpdResponse
            var connectionResult = await CheckConnectionAsync();

            try
            {
                _writer.Write(command);
                foreach (string arg in argument)
                {
                    _writer.Write(' ');
                    WriteToken(arg);
                }
                _writer.WriteLine();
                _writer.Flush();

                return ReadResponse();
            }
            catch (Exception)
            {
                try { await Disconnect(); } catch (Exception) { }
                throw;
            }
        }

        private async Task<bool> CheckConnectionAsync()
        {
            if (!IsConnected)
            {
                await ConnectAsync();
            }

            return IsConnected;
        }

        private void WriteToken(string token)
        {
            if (token.Contains(" "))
            {
                _writer.Write("\"");
                foreach (char chr in token)
                    if (chr == '"')
                        _writer.Write("\\\"");
                    else
                        _writer.Write(chr);
            }
            else
                _writer.Write(token);
        }

        private MpdResponse ReadResponse()
        {
            List<string> ret = new List<string>();
            string line = _reader.ReadLine();
            while (!(line.Equals(OK) || line.StartsWith(ACK)))
            {
                ret.Add(line);
                line = _reader.ReadLine();
            }
            if (line.Equals(OK))
                return new MpdResponse(new ReadOnlyCollection<string>(ret));
            else
            {
                Match match = ACK_REGEX.Match(line);

                if (match.Groups.Count != 5)
                    throw new InvalidDataException( "Error response not as expected" );

                return new MpdResponse(
                    int.Parse( match.Result("${code}") ),
                    int.Parse( match.Result("${nr}") ),
                    match.Result("${command}"),
                    match.Result("${message}"),
                    new ReadOnlyCollection<string>(ret)
                    );
            }
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
