// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandsCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Reflection
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Shows which commands the current user has access to.
    /// config : This command is only permitted to "local" clients (connected via UNIX domain socket).
    /// </summary>
    internal class CommandsCommand : IMpcCommand<IEnumerable<string>>
    {
        public string Serialize() => "commands";

        public IEnumerable<string> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var result = response.Where(item => item.Key.Equals("command")).Select(item => item.Value);

            return result;
        }
    }
}
