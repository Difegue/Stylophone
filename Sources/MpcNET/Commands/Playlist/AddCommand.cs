// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Adds the file URI to the playlist (directories add recursively). URI can also be a single file.
    /// </summary>
    internal class AddCommand : IMpcCommand<string>
    {
        private readonly string uri;

        public AddCommand(string uri)
        {
            this.uri = uri;
        }

        public string Serialize() => string.Join(" ", "add", $"\"{this.uri}\"");

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}
