// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListPlaylistInfoCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Lists the songs with metadata in the playlist.
    /// </summary>
    internal class ListPlaylistInfoCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly string playlistName;

        public ListPlaylistInfoCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        public string Serialize() => string.Join(" ", "listplaylistinfo", $"\"{this.playlistName}\"");

        public IEnumerable<IMpdFile> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }
}