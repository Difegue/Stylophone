// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddIdCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Adds a song to the playlist (non-recursive) and returns the song id.
    /// </summary>
    internal class AddIdCommand : IMpcCommand<string>
    {
        private readonly string uri;

        public AddIdCommand(string uri)
        {
            this.uri = uri;
        }

        public string Serialize() => string.Join(" ", "addid", $"\"{this.uri}\"");

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}