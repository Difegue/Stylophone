using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Libmpc
{
    /// <summary>
    /// Keeps the connection to the MPD server and handels the most basic structure of the
    /// MPD protocol. The high level commands are handeled in the <see cref="Libmpc.Mpc"/>
    /// class.
    /// </summary>
    public class MpcConnection
    {
        /// <summary>
        /// Is fired when a connection to a MPD server is established.
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// Is fired when the connection to the MPD server is closed.
        /// </summary>
        public event EventHandler Disconnected;

        private static readonly string FIRST_LINE_PREFIX = "OK MPD ";

        private static readonly string OK = "OK";
        private static readonly string ACK = "ACK";

        private static readonly Regex ACK_REGEX = new Regex("^ACK \\[(?<code>[0-9]*)@(?<nr>[0-9]*)] \\{(?<command>[a-z]*)} (?<message>.*)$");

        private IPEndPoint _ipEndPoint;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private StreamReader _reader;
        private StreamWriter _writer;

        private string _version;
        /// <summary>
        /// If the connection to the MPD is connected.
        /// </summary>
        public bool IsConnected { get { return (_tcpClient != null) && _tcpClient.Connected; } }
        /// <summary>
        /// The version of the MPD.
        /// </summary>
        public string Version { get { return _version; } }

        private bool _autoConnect = false;
        /// <summary>
        /// If a connection should be established when a command is to be
        /// executed in disconnected state.
        /// </summary>
        public bool AutoConnect
        {
            get{ return _autoConnect; }
            set { _autoConnect = value; }
        }
        
        /// <summary>
        /// Creates a new MpdConnection.
        /// </summary>
        /// <param name="server">The IPEndPoint of the MPD server.</param>
        public MpcConnection(IPEndPoint server)
        {
            Server = server;
        }
        /// <summary>
        /// The IPEndPoint of the MPD server.
        /// </summary>
        /// <exception cref="AlreadyConnectedException">When a conenction to a MPD server is already established.</exception>
        public IPEndPoint Server
        {
            get { return _ipEndPoint; }
            set
            {
                if (IsConnected)
                    throw new AlreadyConnectedException();

                _ipEndPoint = value;

                ClearConnectionFields();
            }
        }
        
        /// <summary>
        /// Connects to the MPD server who's IPEndPoint was set in the Server property.
        /// </summary>
        /// <exception cref="InvalidOperationException">If no IPEndPoint was set to the Server property.</exception>
        public void Connect()
        {
            if (_ipEndPoint == null)
                throw new InvalidOperationException("Server IPEndPoint not set.");

            if (IsConnected)
                throw new AlreadyConnectedException();


            _tcpClient = new TcpClient();
            var connection = _tcpClient.ConnectAsync(_ipEndPoint.Address, _ipEndPoint.Port);
            connection.Wait();

            _networkStream = _tcpClient.GetStream();

            _reader = new StreamReader(_networkStream, Encoding.UTF8);
            _writer = new StreamWriter(_networkStream, Encoding.UTF8);
            _writer.NewLine = "\n";

            string firstLine = _reader.ReadLine();
            if( !firstLine.StartsWith( FIRST_LINE_PREFIX ) )
            {
                Disconnect();
                throw new InvalidDataException("Response of mpd does not start with \"" + FIRST_LINE_PREFIX + "\"." );
            }
            _version = firstLine.Substring(FIRST_LINE_PREFIX.Length);

            _writer.WriteLine();
            _writer.Flush();

            ReadResponse();

            Connected?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Disconnects from the current MPD server.
        /// </summary>
        public void Disconnect()
        {
            if (_tcpClient == null)
                return;

            _networkStream.Dispose();

            ClearConnectionFields();

            Disconnected?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Executes a simple command without arguments on the MPD server and returns the response.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The MPD server response parsed into a basic object.</returns>
        /// <exception cref="ArgumentException">If the command contains a space of a newline charakter.</exception>
        public MpdResponse Exec(string command)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (command.Contains(" "))
                throw new ArgumentException("command contains space");
            if (command.Contains("\n"))
                throw new ArgumentException("command contains newline");

            CheckConnected();

            try
            {
                _writer.WriteLine(command);
                _writer.Flush();

                return ReadResponse();
            }
            catch (Exception)
            {
                try { Disconnect(); }
                catch (Exception) { }
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
        public MpdResponse Exec(string command, string[] argument)
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

            CheckConnected();

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
                try { Disconnect(); } catch (Exception) { }
                throw;
            }
        }

        private void CheckConnected()
        {
            if (!IsConnected)
            {
                if (_autoConnect)
                    Connect();
                else
                    throw new NotConnectedException();
            }

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
            _tcpClient?.Dispose();
            _networkStream?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
            _version = string.Empty;
        }
    }
}
