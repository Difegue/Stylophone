// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpcConnectionReporter.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using System;

    /// <summary>
    /// Interface for implementing an observer for <see cref="MpcConnection"/>.
    /// </summary>
    public interface IMpcConnectionReporter
    {
        /// <summary>
        /// Called when connecting.
        /// </summary>
        /// <param name="isReconnect">if set to <c>true</c> [is reconnect].</param>
        /// <param name="connectAttempt">The connect attempt.</param>
        void Connecting(bool isReconnect, int connectAttempt);

        /// <summary>
        /// Called when connection is accepted.
        /// </summary>
        /// <param name="isReconnect">if set to <c>true</c> [is reconnect].</param>
        /// <param name="connectAttempt">The connect attempt.</param>
        void ConnectionAccepted(bool isReconnect, int connectAttempt);

        /// <summary>
        /// Called when connected.
        /// </summary>
        /// <param name="isReconnect">if set to <c>true</c> [is reconnect].</param>
        /// <param name="connectAttempt">The connect attempt.</param>
        /// <param name="connectionInfo">The connection information.</param>
        void Connected(bool isReconnect, int connectAttempt, string connectionInfo);

        /// <summary>
        /// Called when sending command.
        /// </summary>
        /// <param name="command">The command.</param>
        void Sending(string command);

        /// <summary>
        /// Called when send exception occured.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="sendAttempt">The send attempt.</param>
        /// <param name="exception">The exception.</param>
        void SendException(string commandText, int sendAttempt, Exception exception);

        /// <summary>
        /// Called when send is retried.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="sendAttempt">The send attempt.</param>
        void RetrySend(string command, int sendAttempt);

        /// <summary>
        /// Called when response is read.
        /// </summary>
        /// <param name="responseLine">The response line.</param>
        void ReadResponse(string responseLine);

        /// <summary>
        /// Called when disconnecting.
        /// </summary>
        /// <param name="isExplicitDisconnect">if set to <c>true</c> the disconnect was explicitly called.</param>
        void Disconnecting(bool isExplicitDisconnect);

        /// <summary>
        /// Called when disconnected.
        /// </summary>
        /// <param name="isExplicitDisconnect">if set to <c>true</c> the disconnect was explicitly called.</param>
        void Disconnected(bool isExplicitDisconnect);
    }
}