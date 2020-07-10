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
    /// Command to set repeat state.
    /// https://www.musicpd.org/doc/html/protocol.html#status_commands
    /// </summary>
    public class RepeatCommand : IMpcCommand<string>
    {
        private readonly string playArgument;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatCommand" /> class.
        /// </summary>
        /// <param name="repeat">if set to <c>true</c> [repeat].</param>
        public RepeatCommand(bool repeat)
        {
            this.playArgument = repeat ? "1" : "0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatCommand"/> class.
        /// </summary>
        public RepeatCommand()
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
                return string.Join(" ", "repeat");
            }

            return string.Join(" ", "repeat", this.playArgument);
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