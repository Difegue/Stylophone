// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistCommand.cs" company="Hukano">
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
    /// Displays the current playlist.
    /// </summary>
    internal class PlaylistCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        public string Serialize() => "playlist";

        public IEnumerable<IMpdFile> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var results = response.Select(line => MpdFile.Create(line.Value, int.Parse(line.Key)));

            return results;
        }
    }
}