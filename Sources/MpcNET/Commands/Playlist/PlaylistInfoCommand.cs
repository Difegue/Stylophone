// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistInfoCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Displays a list of all songs in the playlist,
    /// </summary>
    internal class PlaylistInfoCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        public string Serialize() => "playlistinfo";

        public IEnumerable<IMpdFile> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }
}