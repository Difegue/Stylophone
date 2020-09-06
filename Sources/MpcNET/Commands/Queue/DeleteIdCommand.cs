// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteIdCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Queue
{
    using MpcNET;
    using System.Collections.Generic;

    /// <summary>
    /// Deletes the song SONGID from the playlist.
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class DeleteIdCommand : IMpcCommand<string>
    {
        private readonly int songId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteIdCommand"/> class.
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        public DeleteIdCommand(int songId)
        {
            this.songId = songId;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "deleteid", songId);

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