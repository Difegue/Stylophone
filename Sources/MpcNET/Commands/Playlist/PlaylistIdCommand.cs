// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistIdCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Displays song ID in the playlist.
    /// </summary>
    internal class PlaylistIdCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly int songId;

        public PlaylistIdCommand(int songId)
        {
            this.songId = songId;
        }

        public string Serialize() => string.Join(" ", new[] { "playlistid" }, this.songId);

        public IEnumerable<IMpdFile> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }
}