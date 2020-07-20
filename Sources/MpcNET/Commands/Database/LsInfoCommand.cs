// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LsInfoCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Types;

    /// <summary>
    /// Lists the contents of the directory URI. The response contains records starting with file, directory or playlist, each followed by metadata 
    /// https://www.musicpd.org/doc/protocol/database.html.
    /// </summary>
    public class LsInfoCommand : IMpcCommand<IEnumerable<IMpdFilePath>>
    {
        private readonly string uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="LsInfoCommand"/> class.
        /// </summary>
        /// <param name="uri">The uri.</param>
        public LsInfoCommand(string uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => $"lsinfo \"{uri}\"";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFilePath> Deserialize(SerializedResponse response)
        {
            var rootDirectory = new List<IMpdFilePath>();

            foreach (var line in response.ResponseValues)
            {
                // lsinfo can also return playlists, but this is a deprecated behavior I'm entirely willing to not support.

                if (line.Key.Equals("file"))
                {
                    rootDirectory.Add(new MpdFile(line.Value));
                }

                if (line.Key.Equals("directory"))
                {
                    rootDirectory.Add(new MpdDirectory(line.Value));
                }
            }

            return rootDirectory;
        }
    }
}