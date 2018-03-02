// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToggleOutputCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Output
{
    using System.Collections.Generic;

    /// <summary>
    /// Turns an output on or off, depending on the current state.
    /// </summary>
    internal class ToggleOutputCommand : IMpcCommand<string>
    {
        private readonly int outputId;

        public ToggleOutputCommand(int outputId)
        {
            this.outputId = outputId;
        }

        public string Serialize() => string.Join(" ", "toggleoutput", this.outputId);

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}