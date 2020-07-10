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
    /// Seeks to the position TIME (in seconds; fractions allowed) within the current song. 
    /// https://www.musicpd.org/doc/html/protocol.html#status_commands
    /// </summary>
    public class SeekCurCommand : IMpcCommand<string>
    {
        private readonly double time;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeekCurCommand"/> class.
        /// </summary>
        /// <param name="time">The time.</param>
        public SeekCurCommand(double time)
        {
            this.time = time;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            return string.Join(" ", "seekcur", this.time);
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