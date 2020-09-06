// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Queue
{
    using MpcNET;
    using System.Collections.Generic;

    /// <summary>
    /// Moves the song with FROM (songid) to TO (playlist index) in the playlist.
    /// If TO is negative, it is relative to the current song in the playlist (if there is one)
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class MoveIdCommand : IMpcCommand<string>
    {
        private readonly int from;
        private readonly int to;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveIdCommand"/> class.
        /// </summary>
        /// <param name="from">From (songid)</param>
        /// <param name="to">To (playlist index)</param>
        public MoveIdCommand(int from, int to)
        {
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "moveid", from, to);

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
