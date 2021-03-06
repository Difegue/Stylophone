﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisableOutputCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Output
{
    using System.Collections.Generic;

    /// <summary>
    /// Turns an output off.
    /// https://www.musicpd.org/doc/protocol/output_commands.html.
    /// </summary>
    public class DisableOutputCommand : IMpcCommand<string>
    {
        private readonly int outputId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableOutputCommand"/> class.
        /// </summary>
        /// <param name="outputId">The output identifier.</param>
        public DisableOutputCommand(int outputId)
        {
            this.outputId = outputId;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "disableoutput", this.outputId);

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public string Deserialize(SerializedResponse response)
        {
            // Response should be empty.
            return string.Join(", ", response.ResponseValues);
        }
    }
}