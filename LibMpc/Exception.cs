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
using System.IO;
using System.Text;

namespace Libmpc
{
    /// <summary>
    /// Is thrown when a command is to be executed on a disconnected <see cref="MpcConnection"/>
    /// where the <see cref="MpcConnection.AutoConnect"/> property is set to false.
    /// </summary>
    public class NotConnectedException : InvalidOperationException
    {
        /// <summary>
        /// Creates a new NotConnectedException.
        /// </summary>
        public NotConnectedException() : base("Not connected.") {}
    }
    /// <summary>
    /// Is thrown when the connect method is invoked on an already connected <see cref="MpcConnection"/>.
    /// </summary>
    public class AlreadyConnectedException : InvalidOperationException
    {
        /// <summary>
        /// Creates a new AlreadyConnectedException.
        /// </summary>
        public AlreadyConnectedException() : base("Connected already established.") { }
    }
    /// <summary>
    /// Is thrown if the response from a MPD server is not as expected. This should never happen when
    /// working with a tested version of the MPD server.
    /// </summary>
    public class InvalidMpdResponseException : Exception
    {
        /// <summary>
        /// Creates a new InvalidMpdResponseException.
        /// </summary>
        public InvalidMpdResponseException() : base( "Invalid Mpd Response." ) {}
        /// <summary>
        /// Creates a new InvalidMpdResponseException.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        public InvalidMpdResponseException(string message) : base("Invalid Mpd Response: " + message) { }
    }
    /// <summary>
    /// Is thrown when the MPD server returns an error to a command.
    /// </summary>
    public class MpdResponseException : Exception
    {
        private int errorCode;
        private string errorMessage;
        /// <summary>
        /// The error code of the mpd server.
        /// </summary>
        public int ErrorCode { get { return this.errorCode; } }
        /// <summary>
        /// A message describing what went wrong.
        /// </summary>
        public string ErrorMessage { get { return this.errorMessage; } }
        /// <summary>
        /// Creates a new MpdResponseException.
        /// </summary>
        /// <param name="errorCode">The error code of the mpd server.</param>
        /// <param name="errorMessage">A message describing what went wrong.</param>
        public MpdResponseException(int errorCode, string errorMessage)
            : base("MPD" + errorCode + " " + errorMessage)
        {
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
        }
    }
}
