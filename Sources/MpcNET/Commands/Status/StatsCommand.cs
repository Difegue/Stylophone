// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatsCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    using System.Collections.Generic;

    /// <summary>
    /// Get stats from the daemon.
    /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands.
    /// </summary>
    public class StatsCommand : IMpcCommand<Dictionary<string, string>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "stats";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public Dictionary<string, string> Deserialize(SerializedResponse response)
        {
            var result = new Dictionary<string, string>();

            foreach (var pair in response.ResponseValues)
            {
                result.Add(pair.Key, pair.Value);
            }
            return result;
        }
    }
}