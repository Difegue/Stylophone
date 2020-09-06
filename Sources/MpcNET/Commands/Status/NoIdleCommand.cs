// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoIdleCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    /// <summary>
    /// Cancels idle command.
    /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands.
    /// </summary>
    public class NoIdleCommand : IMpcCommand<string>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "noidle";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public string Deserialize(SerializedResponse response)
        {
            return string.Join(", ", response.ResponseValues);
        }
    }
}