// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdRequest.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    /// <summary>
    /// MPD request containing the command.
    /// </summary>
    /// <typeparam name="TContent">The content of the reponse.</typeparam>
    /// <seealso cref="Message.IMpdRequest{T}" />
    public class MpdRequest<TContent> : IMpdRequest<TContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdRequest{T}"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        public MpdRequest(IMpcCommand<TContent> command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public IMpcCommand<TContent> Command { get; }
    }
}