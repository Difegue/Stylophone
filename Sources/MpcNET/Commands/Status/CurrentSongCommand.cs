// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentSongCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    using System.Collections.Generic;
    using MpcNET.Types;

    internal class CurrentSongCommand : IMpcCommand<IMpdFile>
    {
        public string Serialize() => "currentsong";

        public IMpdFile Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return MpdFile.Create(response, 0).mpdFile;
        }
    }
}