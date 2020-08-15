// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpcConnection.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MpcNET.Message;

    /// <summary>
    /// Interface for implementing an MPD connection.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IMpcConnection : IDisposable
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Connects asynchronously.
        /// </summary>
        /// <returns>The connect task.</returns>
        Task ConnectAsync(CancellationToken token);

        /// <summary>
        /// Disconnects asynchronously.
        /// </summary>
        /// <returns>The disconnect task.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Sends the command asynchronously.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="mpcCommand">The command selector.</param>
        /// <returns>
        /// The send task.
        /// </returns>
        Task<IMpdMessage<TResponse>> SendAsync<TResponse>(IMpcCommand<TResponse> mpcCommand);
    }
}