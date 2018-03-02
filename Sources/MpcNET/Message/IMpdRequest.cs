// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdRequest.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    /// <summary>
    /// Interface for implementing a MPD request.
    /// </summary>
    /// <typeparam name="TContent">The response content.</typeparam>
    public interface IMpdRequest<out TContent>
    {
        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        IMpcCommand<TContent> Command { get; }
    }
}