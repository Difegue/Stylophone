// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentPlaylistCommandFactory.cs" company="Hukano">
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
    /// https://www.musicpd.org/doc/protocol/queue.html
    /// </summary>
    public class CurrentPlaylistCommandFactory : ICurrentPlaylistCommandFactory
    {
        /// <summary>
        /// Command: add
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>An <see cref="AddCommand"/>.</returns>
        public IMpcCommand<string> AddDirectory(string directory)
        {
            return new AddCommand(directory);
        }

        /// <summary>
        /// Command: addid
        /// </summary>
        /// <param name="songPath">The song path.</param>
        /// <returns>An <see cref="AddIdCommand"/>.</returns>
        public IMpcCommand<string> AddSong(string songPath)
        {
            return new AddIdCommand(songPath);
        }

        /// <summary>
        /// Command: clear
        /// </summary>
        /// <returns>An <see cref="ClearCommand"/>.</returns>
        public IMpcCommand<string> Clear()
        {
            return new ClearCommand();
        }

        /// <summary>
        /// Command: playlist
        /// </summary>
        /// <returns>A <see cref="PlaylistCommand"/>.</returns>
        public IMpcCommand<IEnumerable<IMpdFile>> GetAllSongsInfo()
        {
            return new PlaylistCommand();
        }

        /// <summary>
        /// Command: delete
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>A <see cref="DeleteCommand" />.</returns>
        public IMpcCommand<string> RemoveSongByPosition(int position)
        {
            return new DeleteCommand(position);
        }

        /// <summary>
        /// Command: deleteid
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        /// <returns>A <see cref="DeleteIdCommand"/>.</returns>
        public IMpcCommand<string> RemoveSongById(int songId)
        {
            return new DeleteIdCommand(songId);
        }

        /// <summary>
        /// Command: playlistid
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        /// <returns>A <see cref="PlaylistIdCommand" />.</returns>
        public IMpcCommand<IEnumerable<IMpdFile>> GetSongMetadata(int songId)
        {
            return new PlaylistIdCommand(songId);
        }

        /// <summary>
        /// Command: playlistinfo
        /// </summary>
        /// <returns>A <see cref="PlaylistInfoCommand" />.</returns>
        public IMpcCommand<IEnumerable<IMpdFile>> GetAllSongMetadata()
        {
            return new PlaylistInfoCommand();
        }
    }
}