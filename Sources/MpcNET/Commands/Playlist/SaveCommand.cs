// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Saves the queue to NAME.m3u in the playlist directory.
    /// https://www.musicpd.org/doc/html/protocol.html#stored-playlists
    /// </summary>
    public class SaveCommand : IMpcCommand<string>
    {
        private readonly string playlistName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommand"/> class.
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        public SaveCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "save", $"\"{this.playlistName}\"");

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