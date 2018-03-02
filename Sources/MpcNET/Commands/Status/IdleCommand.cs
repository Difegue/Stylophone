// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdleCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    using System.Collections.Generic;

    internal class IdleCommand : IMpcCommand<string>
    {
        private readonly string subSystem;

        public IdleCommand(string subSystem)
        {
            this.subSystem = subSystem;
        }

        public string Serialize()
        {
            if (string.IsNullOrEmpty(this.subSystem))
            {
                return "idle";
            }

            return "idle " + this.subSystem;
        }

        public string Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}