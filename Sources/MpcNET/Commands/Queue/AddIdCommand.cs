// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddIdCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Queue
{
    using MpcNET;
    using System.Collections.Generic;

    /// <summary>
    /// Adds a song to the playlist (non-recursive) and returns the song id.
    /// https://www.musicpd.org/doc/protocol/queue.html.
    /// </summary>
    public class AddIdCommand : IMpcCommand<string>
    {
        private readonly string uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddIdCommand"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public AddIdCommand(string uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "addid", $"\"{uri}\"");

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