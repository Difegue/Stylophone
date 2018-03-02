// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Clears the current playlist.
    /// </summary>
    internal class ClearCommand : IMpcCommand<string>
    {
        public string Serialize() => "clear";

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}