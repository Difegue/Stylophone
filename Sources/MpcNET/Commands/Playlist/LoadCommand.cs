// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoadCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;

    /// <summary>
    /// Loads the playlist into the current queue.
    /// https://www.musicpd.org/doc/protocol/playlist_files.html.
    /// </summary>
    public class LoadCommand : IMpcCommand<string>
    {
        private readonly string playlistName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadCommand"/> class.
        /// </summary>
        /// <param name="playlistName">Name of the playlist.</param>
        public LoadCommand(string playlistName)
        {
            this.playlistName = playlistName;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "load", $"\"{this.playlistName}\"");

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