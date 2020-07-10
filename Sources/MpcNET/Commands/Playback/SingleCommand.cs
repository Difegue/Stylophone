// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PauseResumeCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playback
{
    using System.Collections.Generic;

    /// <summary>
    /// Command to set single state. 
    /// When single is activated, playback is stopped after current song, or song is repeated if the ‘repeat’ mode is enabled.
    /// https://www.musicpd.org/doc/html/protocol.html#status_commands
    /// </summary>
    public class SingleCommand : IMpcCommand<string>
    {
        private readonly string playArgument;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleCommand" /> class.
        /// </summary>
        /// <param name="single">if set to <c>true</c> [single].</param>
        public SingleCommand(bool single)
        {
            this.playArgument = single ? "1" : "0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleCommand"/> class.
        /// </summary>
        public SingleCommand()
        {
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            if (this.playArgument == null)
            {
                return string.Join(" ", "single");
            }

            return string.Join(" ", "single", this.playArgument);
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