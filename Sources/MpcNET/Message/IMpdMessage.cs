// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdMessage.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    /// <summary>
    /// Interface for implementing MPD message.
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    public interface IMpdMessage<TContent>
    {
        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        IMpdRequest<TContent> Request { get; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        IMpdResponse<TContent> Response { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is response valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is response valid; otherwise, <c>false</c>.
        /// </value>
        bool IsResponseValid { get; }
    }
}