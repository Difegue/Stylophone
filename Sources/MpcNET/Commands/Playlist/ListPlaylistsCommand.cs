// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListPlaylistsCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playlist
{
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Types;

    /// <summary>
    /// Prints a list of the playlist directory.
    /// https://www.musicpd.org/doc/protocol/playlist_files.html.
    /// </summary>
    public class ListPlaylistsCommand : IMpcCommand<IEnumerable<MpdPlaylist>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "listplaylists";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<MpdPlaylist> Deserialize(SerializedResponse response)
        {
            var result = new List<MpdPlaylist>();

            foreach (var line in response.ResponseValues)
            {
                if (line.Key.Equals("playlist"))
                {
                    result.Add(new MpdPlaylist(line.Value));
                }
                else if (line.Key.Equals("Last-Modified"))
                {
                    result.Last().AddLastModified(line.Value);
                }
            }

            return result;
        }
    }
}