// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListPlaylistCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Types;

    /// <summary>
    /// Lists the songs in the playlist.
    /// https://www.musicpd.org/doc/protocol/playlist_files.html.
    /// </summary>
    public class ListPlaylistCommand : IMpcCommand<IEnumerable<IMpdFilePath>>
    {
        private readonly string playlistName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListPlaylistCommand"/> class.
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        public ListPlaylistCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "listplaylist", $"\"{this.playlistName}\"");

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFilePath> Deserialize(SerializedResponse response)
        {
            var results = response.ResponseValues.Where(line => line.Key.Equals("file")).Select(line => new MpdFile(line.Value));

            return results;
        }
    }
}