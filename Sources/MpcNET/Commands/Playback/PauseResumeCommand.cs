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
    /// Command to pause or resume.
    /// https://www.musicpd.org/doc/protocol/playback_commands.html.
    /// </summary>
    public class PauseResumeCommand : IMpcCommand<string>
    {
        private readonly string playArgument;

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseResumeCommand" /> class.
        /// </summary>
        /// <param name="pause">if set to <c>true</c> [pause].</param>
        public PauseResumeCommand(bool pause)
        {
            this.playArgument = pause ? "1" : "0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseResumeCommand"/> class.
        /// </summary>
        public PauseResumeCommand()
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
                return string.Join(" ", "pause");
            }

            return string.Join(" ", "pause", this.playArgument);
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