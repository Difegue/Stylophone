// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Deletes a song from the playlist.
    /// </summary>
    internal class DeleteCommand : IMpcCommand<string>
    {
        private readonly int position;

        public DeleteCommand(int position)
        {
            this.position = position;
        }

        public string Serialize() => string.Join(" ", "delete", this.position);

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}