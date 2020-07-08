// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdleCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    using System.Collections.Generic;

    /// <summary>
    /// Idles mpd until something happens.
    /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands.
    /// </summary>
    public class IdleCommand : IMpcCommand<string>
    {
        private readonly string subSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdleCommand"/> class.
        /// </summary>
        /// <param name="subSystem">The sub system.</param>
        public IdleCommand(string subSystem)
        {
            this.subSystem = subSystem;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            if (string.IsNullOrEmpty(this.subSystem))
            {
                return "idle";
            }

            return "idle " + this.subSystem;
        }

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