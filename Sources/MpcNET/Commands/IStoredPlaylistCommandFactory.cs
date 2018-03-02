// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStoredPlaylistCommandFactory.cs" company="Hukano">
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
    /// Provides stored playlist commands.
    /// </summary>
    public interface IStoredPlaylistCommandFactory
    {
        /// <summary>
        /// Command: load
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        /// <returns>A <see cref="LoadCommand" />.</returns>
        IMpcCommand<string> Load(string playlistName);

        /// <summary>
        /// Command: listplaylist
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        /// <returns>A <see cref="ListPlaylistCommand" />.</returns>
        IMpcCommand<IEnumerable<IMpdFilePath>> GetContent(string playlistName);

        /// <summary>
        /// Command: listplaylistinfo
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        /// <returns>A <see cref="ListPlaylistInfoCommand" />.</returns>
        IMpcCommand<IEnumerable<IMpdFile>> GetContentWithMetadata(string playlistName);

        /// <summary>
        /// Command: listplaylists
        /// </summary>
        /// <returns>A <see cref="ListPlaylistsCommand" />.</returns>
        IMpcCommand<IEnumerable<MpdPlaylist>> GetAll();
    }
}