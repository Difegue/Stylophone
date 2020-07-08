// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playback
{
    using System;
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Command to start playback.
    /// https://www.musicpd.org/doc/protocol/playback_commands.html.
    /// </summary>
    public class PlayCommand : IMpcCommand<string>
    {
        private readonly int position;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayCommand" /> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public PlayCommand(int position)
        {
            this.position = position;
            if (this.position == MpdFile.NoPos)
            {
                throw new ArgumentException("PlayCommand requires Position");
            }
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            return string.Join(" ", "play", this.position);
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