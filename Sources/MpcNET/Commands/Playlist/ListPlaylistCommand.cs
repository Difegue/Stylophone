// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListPlaylistCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
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
    /// </summary>
    internal class ListPlaylistCommand : IMpcCommand<IEnumerable<IMpdFilePath>>
    {
        private readonly string playlistName;

        public ListPlaylistCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        public string Serialize() => string.Join(" ", "listplaylist", $"\"{this.playlistName}\"");

        public IEnumerable<IMpdFilePath> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var results = response.Where(line => line.Key.Equals("file")).Select(line => new MpdFile(line.Value));

            return results;
        }
    }
}