// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisableOutputCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Output
{
    using System.Collections.Generic;

    /// <summary>
    /// Turns an output off.
    /// </summary>
    internal class DisableOutputCommand : IMpcCommand<string>
    {
        private readonly int outputId;

        public DisableOutputCommand(int outputId)
        {
            this.outputId = outputId;
        }

        public string Serialize() => string.Join(" ", "disableoutput", this.outputId);

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            // Response should be empty.
            return string.Join(", ", response);
        }
    }
}