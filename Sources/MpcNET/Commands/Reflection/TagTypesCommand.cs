// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagTypesCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Reflection
{
    using System.Collections.Generic;
    using System.Linq;

    // TODO: notcommands : Shows which commands the current user does not have access to.

    /// <summary>
    /// Shows a list of available song metadata.
    /// </summary>
    internal class TagTypesCommand : IMpcCommand<IEnumerable<string>>
    {
        public string Serialize() => "tagtypes";

        public IEnumerable<string> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var result = response.Where(item => item.Key.Equals("tagtype")).Select(item => item.Value);

            return result;
        }
    }
}