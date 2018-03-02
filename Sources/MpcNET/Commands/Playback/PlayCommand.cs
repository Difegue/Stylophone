// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playback
{
    using System;
    using System.Collections.Generic;
    using MpcNET.Types;

    internal class PlayCommand : IMpcCommand<string>
    {
        private readonly int position;
        private readonly int id;

        public PlayCommand(int position, int id)
        {
            this.position = position;
            this.id = id;
            if (this.position == MpdFile.NoPos && this.id == MpdFile.NoId)
            {
                throw new ArgumentException("PlayCommand requires Id or Position");
            }
        }

        public string Serialize()
        {
            if (this.id != MpdFile.NoId)
            {
                return string.Join(" ", "playid", this.id);
            }

            return string.Join(" ", "play", this.position);
        }

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}