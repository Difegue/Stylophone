// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICurrentPlaylistCommandFactory.cs" company="Hukano">
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
    /// Provides current playlist commands.
    /// </summary>
    public interface ICurrentPlaylistCommandFactory
    {
        /// <summary>
        /// Command: add
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>An <see cref="AddCommand"/>.</returns>
        IMpcCommand<string> AddDirectory(string directory);

        /// <summary>
        /// Command: addid
        /// </summary>
        /// <param name="songPath">The song path.</param>
        /// <returns>An <see cref="AddIdCommand"/>.</returns>
        IMpcCommand<string> AddSong(string songPath);

        /// <summary>
        /// Command: clear
        /// </summary>
        /// <returns>An <see cref="ClearCommand"/>.</returns>
        IMpcCommand<string> Clear();

        /// <summary>
        /// Command: playlist
        /// </summary>
        /// <returns>A <see cref="PlaylistCommand"/>.</returns>
        IMpcCommand<IEnumerable<IMpdFile>> GetAllSongsInfo();

        /// <summary>
        /// Command: delete
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>A <see cref="DeleteCommand" />.</returns>
        IMpcCommand<string> RemoveSongByPosition(int position);

        /// <summary>
        /// Command: deleteid
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        /// <returns>A <see cref="DeleteIdCommand"/>.</returns>
        IMpcCommand<string> RemoveSongById(int songId);

        /// <summary>
        /// Command: playlistid
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        /// <returns>A <see cref="PlaylistIdCommand" />.</returns>
        IMpcCommand<IEnumerable<IMpdFile>> GetSongMetadata(int songId);

        /// <summary>
        /// Command: playlistinfo
        /// </summary>
        /// <returns>A <see cref="PlaylistInfoCommand" />.</returns>
        IMpcCommand<IEnumerable<IMpdFile>> GetAllSongMetadata();
    }
}