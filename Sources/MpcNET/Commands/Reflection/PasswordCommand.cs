// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Reflection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Command to authenticate with a password.
    /// https://mpd.readthedocs.io/en/stable/protocol.html#connection-settings.
    /// </summary>
    public class PasswordCommand : IMpcCommand<string>
    {
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordCommand"/> class.
        /// </summary>
        /// <param name="pass">The password.</param>
        public PasswordCommand(string pass)
        {
            _password = pass;
            if (_password == "")
            {
                throw new ArgumentException("Empty string given to PasswordCommand");
            }
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialized command.
        /// </returns>
        public string Serialize() => string.Join(" ", new[] { "password", _password });

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public string Deserialize(SerializedResponse response)
        {
            return string.Join(", ", response.ResponseValues);
        }
    }
}