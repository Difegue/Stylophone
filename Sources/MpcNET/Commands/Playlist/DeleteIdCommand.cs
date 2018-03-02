// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteIdCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Deletes the song SONGID from the playlist
    /// </summary>
    internal class DeleteIdCommand : IMpcCommand<string>
    {
        private readonly int songId;

        public DeleteIdCommand(int songId)
        {
            this.songId = songId;
        }

        public string Serialize() => string.Join(" ", "deleteid", this.songId);

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}