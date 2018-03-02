// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnableOutputCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Output
{
    using System.Collections.Generic;

    /// <summary>
    /// Turns an output on.
    /// </summary>
    internal class EnableOutputCommand : IMpcCommand<string>
    {
        private readonly int outputId;

        public EnableOutputCommand(int outputId)
        {
            this.outputId = outputId;
        }

        public string Serialize() => string.Join(" ", "enableoutput", this.outputId);

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            // Response should be empty.
            return string.Join(", ", response);
        }
    }
}