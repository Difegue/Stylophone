// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoadCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Loads the playlist into the current queue.
    /// </summary>
    internal class LoadCommand : IMpcCommand<string>
    {
        private readonly string playlistName;

        public LoadCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        public string Serialize() => string.Join(" ", "load", $"\"{this.playlistName}\"");

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}