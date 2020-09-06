// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlChangesCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET.Commands.Queue
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Displays changed songs currently in the playlist since VERSION. 
    /// https://www.musicpd.org/doc/html/protocol.html#the-queue.
    /// </summary>
    public class PlChangesCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly string version;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlChangesCommand"/> class.
        /// </summary>
        /// <param name="version">Version to compare to the current playlist.</param>
        public PlChangesCommand(int version = -1)
        {
            this.version = version.ToString();
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => $"plchanges {version}";

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