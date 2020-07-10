// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PauseResumeCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Playback
{
    using System.Collections.Generic;

    /// <summary>
    /// Command to set random state.
    /// https://www.musicpd.org/doc/html/protocol.html#playback-options
    /// </summary>
    public class RandomCommand : IMpcCommand<string>
    {
        private readonly string playArgument;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomCommand" /> class.
        /// </summary>
        /// <param name="random">if set to <c>true</c> [random].</param>
        public RandomCommand(bool random)
        {
            this.playArgument = random ? "1" : "0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomCommand"/> class.
        /// </summary>
        public RandomCommand()
        {
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            if (this.playArgument == null)
            {
                return string.Join(" ", "random");
            }

            return string.Join(" ", "random", this.playArgument);
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