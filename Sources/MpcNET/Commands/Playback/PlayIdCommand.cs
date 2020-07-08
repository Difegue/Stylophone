// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayIdCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playback
{
    using System;
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Command to start playback.
    /// https://www.musicpd.org/doc/protocol/playback_commands.html.
    /// </summary>
    public class PlayIdCommand : IMpcCommand<string>
    {
        private readonly int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayIdCommand"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public PlayIdCommand(int id)
        {
            this.id = id;
            if (this.id == MpdFile.NoId)
            {
                throw new ArgumentException("PlayIdCommand requires Id");
            }
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            return string.Join(" ", "playid", this.id);
        }

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