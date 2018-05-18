// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdResponseResult.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    /// <summary>
    /// Interface for implementing a MPD response result.
    /// </summary>
    public interface IMpdResponseResult
    {
        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        string Status { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets the MPD error.
        /// </summary>
        /// <value>
        /// The MPD error.
        /// </value>
        string MpdError { get; }

        /// <summary>
        /// Gets a value indicating whether an error occured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if error; otherwise, <c>false</c>.
        /// </value>
        bool Error { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMpdResponseResult"/> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        bool Connected { get; }
    }
}