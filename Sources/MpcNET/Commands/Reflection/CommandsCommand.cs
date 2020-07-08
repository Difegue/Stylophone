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
    /// https://www.musicpd.org/doc/protocol/reflection_commands.html.
    /// </summary>
    public class CommandsCommand : IMpcCommand<IEnumerable<string>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "commands";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<string> Deserialize(SerializedResponse response)
        {
            var result = response.ResponseValues.Where(item => item.Key.Equals("command")).Select(item => item.Value);

            return result;
        }
    }
}
