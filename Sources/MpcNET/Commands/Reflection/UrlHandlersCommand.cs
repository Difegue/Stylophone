// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlHandlersCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Reflection
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Gets a list of available URL handlers.
    /// </summary>
    internal class UrlHandlersCommand : IMpcCommand<IEnumerable<string>>
    {
        public string Serialize() => "urlhandlers";

        public IEnumerable<string> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var result = response.Where(item => item.Key.Equals("handler")).Select(item => item.Value);

            return result;
        }
    }
}