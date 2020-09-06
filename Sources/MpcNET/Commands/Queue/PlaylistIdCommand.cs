// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistIdCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Queue
{
    using System.Collections.Generic;
    using MpcNET;
    using MpcNET.Types;

    /// <summary>
    /// Displays song ID in the playlist.
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class PlaylistIdCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly int songId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistIdCommand"/> class.
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        public PlaylistIdCommand(int songId)
        {
            this.songId = songId;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "playlistid", songId);

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFile> Deserialize(SerializedResponse response)
        {
            return MpdFile.CreateList(response.ResponseValues);
        }
    }
}