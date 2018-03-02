// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoredPlaylistCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using System.Collections.Generic;
    using MpcNET.Commands.Playlist;
    using MpcNET.Types;

    /// <summary>
    /// https://www.musicpd.org/doc/protocol/playlist_files.html
    /// </summary>
    public class StoredPlaylistCommandFactory : IStoredPlaylistCommandFactory
    {
        /// <summary>
        /// Command: load
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        /// <returns>A <see cref="LoadCommand" />.</returns>
        public IMpcCommand<string> Load(string playlistName)
        {
            return new LoadCommand(playlistName);
        }

        /// <summary>
        /// Command: listplaylist
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        /// <returns>A <see cref="ListPlaylistCommand" />.</returns>
        public IMpcCommand<IEnumerable<IMpdFilePath>> GetContent(string playlistName)
        {
            return new ListPlaylistCommand(playlistName);
        }

        /// <summary>
        /// Command: listplaylistinfo
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        /// <returns>A <see cref="ListPlaylistInfoCommand" />.</returns>
        public IMpcCommand<IEnumerable<IMpdFile>> GetContentWithMetadata(string playlistName)
        {
            return new ListPlaylistInfoCommand(playlistName);
        }

        /// <summary>
        /// Command: listplaylists
        /// </summary>
        /// <returns>A <see cref="ListPlaylistsCommand" />.</returns>
        public IMpcCommand<IEnumerable<MpdPlaylist>> GetAll()
        {
            return new ListPlaylistsCommand();
        }
    }
}