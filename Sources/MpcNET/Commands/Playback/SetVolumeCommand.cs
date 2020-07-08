// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetVolumeCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET.Commands.Playback
{
    using System.Collections.Generic;

    /// <summary>
    /// Command to set volume.
    /// https://www.musicpd.org/doc/protocol/playback_commands.html.
    /// </summary>
    public class SetVolumeCommand : IMpcCommand<string>
    {
        private readonly byte volume;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetVolumeCommand"/> class.
        /// </summary>
        /// <param name="volume">The volume.</param>
        public SetVolumeCommand(byte volume)
        {
            this.volume = volume;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            return string.Join(" ", "setvol", this.volume);
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