// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Queue
{
    using MpcNET;
    using System.Collections.Generic;

    /// <summary>
    /// Deletes a song from the playlist.
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class DeleteCommand : IMpcCommand<string>
    {
        private readonly int position;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommand"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public DeleteCommand(int position)
        {
            this.position = position;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "delete", position);

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