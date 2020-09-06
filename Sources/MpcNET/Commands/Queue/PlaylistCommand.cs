// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaylistCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET.Commands.Queue
{
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Types;

    /// <summary>
    /// Displays the current playlist.
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class PlaylistCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "playlist";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFile> Deserialize(SerializedResponse response)
        {
            var results = response.ResponseValues.Select(line => MpdFile.Create(line.Value, int.Parse(line.Key)));

            return results;
        }
    }
}