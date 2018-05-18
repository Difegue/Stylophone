// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListPlaylistsCommand.cs" company="MpcNET">
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
    /// Prints a list of the playlist directory.
    /// </summary>
    internal class ListPlaylistsCommand : IMpcCommand<IEnumerable<MpdPlaylist>>
    {
        public string Serialize() => "listplaylists";

        public IEnumerable<MpdPlaylist> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var result = new List<MpdPlaylist>();

            foreach (var line in response)
            {
                if (line.Key.Equals("playlist"))
                {
                    result.Add(new MpdPlaylist(line.Value));
                }
                else if (line.Key.Equals("Last-Modified"))
                {
                    result.Last().AddLastModified(line.Value);
                }
            }

            return result;
        }
    }
}