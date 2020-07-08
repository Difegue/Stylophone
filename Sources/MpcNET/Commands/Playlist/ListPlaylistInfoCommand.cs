// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListPlaylistInfoCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Lists the songs with metadata in the playlist.
    /// https://www.musicpd.org/doc/protocol/playlist_files.html.
    /// </summary>
    public class ListPlaylistInfoCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly string playlistName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListPlaylistInfoCommand"/> class.
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        public ListPlaylistInfoCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "listplaylistinfo", $"\"{this.playlistName}\"");

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