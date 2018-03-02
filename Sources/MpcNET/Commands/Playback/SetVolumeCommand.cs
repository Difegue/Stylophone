// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetVolumeCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET.Commands.Playback
{
    using System.Collections.Generic;

    internal class SetVolumeCommand : IMpcCommand<string>
    {
        private readonly byte volume;

        public SetVolumeCommand(byte volume)
        {
            this.volume = volume;
        }

        public string Serialize()
        {
            return string.Join(" ", "setvol", this.volume);
        }

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}