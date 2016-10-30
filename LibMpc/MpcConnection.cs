/*
 * Copyright 2008 Matthias Sessler
 * 
 * This file is part of LibMpc.net.
 *
 * LibMpc.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * LibMpc.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with LibMpc.net.  If not, see <http://www.gnu.org/licenses/>.
 */
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
    /// The delegate for the <see cref="MpcConnection.OnConnected"/> and <see cref="MpcConnection.OnDisconnected"/> events.
    /// </summary>
    /// <param name="connection">The connection firing the event.</param>
    public delegate void MpcConnectionEventDelegate( MpcConnection connection );
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
        public event MpcConnectionEventDelegate OnConnected;
        /// <summary>
        /// Is fired when the connection to the MPD server is closed.
        /// </summary>
        public event MpcConnectionEventDelegate OnDisconnected;

        private static readonly string FIRST_LINE_PREFIX = "OK MPD ";

        private static readonly string OK = "OK";
        private static readonly string ACK = "ACK";

        private static readonly Regex ACK_REGEX = new Regex("^ACK \\[(?<code>[0-9]*)@(?<nr>[0-9]*)] \\{(?<command>[a-z]*)} (?<message>.*)$");

        private IPEndPoint ipEndPoint = null;

        private TcpClient tcpClient = null;
        private NetworkStream networkStream = null;

        private StreamReader reader;
        private StreamWriter writer;

        private string version;
        /// <summary>
        /// If the connection to the MPD is connected.
        /// </summary>
        public bool Connected { get { return (this.tcpClient != null) && this.tcpClient.Connected; } }
        /// <summary>
        /// The version of the MPD.
        /// </summary>
        public string Version { get { return this.version; } }

        private bool autoConnect = false;
        /// <summary>
        /// If a connection should be established when a command is to be
        /// executed in disconnected state.
        /// </summary>
        public bool AutoConnect
        {
            get{ return this.autoConnect; }
            set { this.autoConnect = value; }
        }
        /// <summary>
        /// Creates a new MpdConnection.
        /// </summary>
        public MpcConnection() {}
        /// <summary>
        /// Creates a new MpdConnection.
        /// </summary>
        /// <param name="server">The IPEndPoint of the MPD server.</param>
        public MpcConnection(IPEndPoint server) { this.Connect(server); }
        /// <summary>
        /// The IPEndPoint of the MPD server.
        /// </summary>
        /// <exception cref="AlreadyConnectedException">When a conenction to a MPD server is already established.</exception>
        public IPEndPoint Server
        {
            get { return this.ipEndPoint; }
            set
            {
                if (this.Connected)
                    throw new AlreadyConnectedException();

                this.ipEndPoint = value;

                this.ClearConnectionFields();
            }
        }
        /// <summary>
        /// Connects to a MPD server.
        /// </summary>
        /// <param name="server">The IPEndPoint of the server.</param>
        public void Connect(IPEndPoint server)
        {
            this.Server = server;
            this.Connect();
        }
        /// <summary>
        /// Connects to the MPD server who's IPEndPoint was set in the Server property.
        /// </summary>
        /// <exception cref="InvalidOperationException">If no IPEndPoint was set to the Server property.</exception>
        public void Connect()
        {
            if (this.ipEndPoint == null)
                throw new InvalidOperationException("Server IPEndPoint not set.");

            if (this.Connected)
                throw new AlreadyConnectedException();

            this.tcpClient = new TcpClient(
                this.ipEndPoint.Address.ToString(), 
                this.ipEndPoint.Port);
            this.networkStream = this.tcpClient.GetStream();

            this.reader = new StreamReader(this.networkStream, Encoding.UTF8);
            this.writer = new StreamWriter(this.networkStream, Encoding.UTF8);
            this.writer.NewLine = "\n";

            string firstLine = this.reader.ReadLine();
            if( !firstLine.StartsWith( FIRST_LINE_PREFIX ) )
            {
                this.Disconnect();
                throw new InvalidDataException("Response of mpd does not start with \"" + FIRST_LINE_PREFIX + "\"." );
            }
            this.version = firstLine.Substring(FIRST_LINE_PREFIX.Length);

            this.writer.WriteLine();
            this.writer.Flush();

            this.readResponse();

            if( this.OnConnected != null )
                this.OnConnected.Invoke( this );
        }
        /// <summary>
        /// Disconnects from the current MPD server.
        /// </summary>
        public void Disconnect()
        {
            if (this.tcpClient == null)
                return;

            this.networkStream.Close();

            this.ClearConnectionFields();

            if( this.OnDisconnected != null )
                this.OnDisconnected.Invoke( this );
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

            this.CheckConnected();

            try
            {
                this.writer.WriteLine(command);
                this.writer.Flush();

                return this.readResponse();
            }
            catch (Exception)
            {
                try { this.Disconnect(); }
                catch (Exception) { }
                throw;
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

            this.CheckConnected();

            try
            {
                this.writer.Write(command);
                foreach (string arg in argument)
                {
                    this.writer.Write(' ');
                    this.WriteToken(arg);
                }
                this.writer.WriteLine();
                this.writer.Flush();

                return this.readResponse();
            }
            catch (Exception)
            {
                try { this.Disconnect(); } catch (Exception) { }
                throw;
            }
        }

        private void CheckConnected()
        {
            if (!this.Connected)
            {
                if (this.autoConnect)
                    this.Connect();
                else
                    throw new NotConnectedException();
            }

        }

        private void WriteToken(string token)
        {
            if (token.Contains(" "))
            {
                this.writer.Write("\"");
                foreach (char chr in token)
                    if (chr == '"')
                        this.writer.Write("\\\"");
                    else
                        this.writer.Write(chr);
            }
            else
                this.writer.Write(token);
        }

        private MpdResponse readResponse()
        {
            List<string> ret = new List<string>();
            string line = this.reader.ReadLine();
            while (!(line.Equals(OK) || line.StartsWith(ACK)))
            {
                ret.Add(line);
                line = this.reader.ReadLine();
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
            this.tcpClient = null;
            this.networkStream = null;
            this.reader = null;
            this.writer = null;
            this.version = null;
        }
    }
}
