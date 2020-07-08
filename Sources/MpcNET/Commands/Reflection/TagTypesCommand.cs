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
    /// https://www.musicpd.org/doc/protocol/reflection_commands.html.
    /// </summary>
    public class TagTypesCommand : IMpcCommand<IEnumerable<string>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "tagtypes";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<string> Deserialize(SerializedResponse response)
        {
            var result = response.ResponseValues.Where(item => item.Key.Equals("tagtype")).Select(item => item.Value);

            return result;
        }
    }
}