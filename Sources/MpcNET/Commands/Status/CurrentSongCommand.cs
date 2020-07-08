// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentSongCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Gets the current song.
    /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands.
    /// </summary>
    public class CurrentSongCommand : IMpcCommand<IMpdFile>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "currentsong";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IMpdFile Deserialize(SerializedResponse response)
        {
            return MpdFile.Create(response.ResponseValues, 0).mpdFile;
        }
    }
}