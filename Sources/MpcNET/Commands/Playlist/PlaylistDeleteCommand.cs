// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistDeleteCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{

    /// <summary>
    /// Deletes SONGPOS from the playlist NAME.m3u.
    /// https://www.musicpd.org/doc/html/protocol.html#stored-playlists
    /// </summary>
    public class PlaylistDeleteCommand : IMpcCommand<string>
    {
        private readonly string playlist;
        private readonly int songpos;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistDeleteCommand"/> class.
        /// </summary>
        /// <param name="playlistName">The playlist name.</param>
        /// <param name="songpos">Position of the song to remove</param>
        public PlaylistDeleteCommand(string playlistName, int songpos)
        {
            this.playlist = playlistName;
            this.songpos = songpos;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "playlistdelete", $"\"{playlist}\"", songpos);

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