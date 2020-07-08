// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpcCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for implementing a MPD command.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IMpcCommand<out TValue>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>The serialize command.</returns>
        string Serialize();

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The deserialized response.</returns>
        TValue Deserialize(SerializedResponse response);
    }
}