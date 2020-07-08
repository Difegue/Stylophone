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
    /// https://www.musicpd.org/doc/protocol/reflection_commands.html.
    /// </summary>
    public class UrlHandlersCommand : IMpcCommand<IEnumerable<string>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "urlhandlers";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<string> Deserialize(SerializedResponse response)
        {
            var result = response.ResponseValues.Where(item => item.Key.Equals("handler")).Select(item => item.Value);

            return result;
        }
    }
}