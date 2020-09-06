// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistInfoCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET.Commands.Queue
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Displays a list of all songs in the playlist.
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class PlaylistInfoCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "playlistinfo";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFile> Deserialize(SerializedResponse response)
        {
            return MpdFile.CreateList(response.ResponseValues);
        }
    }
}