// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdResponse.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    /// <summary>
    /// Represents a response to a <see cref="IMpcCommand{TResponse}"/>.
    /// </summary>
    /// <typeparam name="TContent">The content type.</typeparam>
    /// <seealso cref="MpcNET.Message.IMpdResponse{T}" />
    public class MpdResponse<TContent> : IMpdResponse<TContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdResponse{T}"/> class.
        /// </summary>
        /// <param name="endLine">The end line.</param>
        /// <param name="content">The content.</param>
        /// <param name="connected">if set to <c>true</c> [connected].</param>
        public MpdResponse(string endLine, TContent content, bool connected)
        {
            this.Result = new MpdResponseResult(endLine, connected, null);
            this.Content = content;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public IMpdResponseResult Result { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public TContent Content { get; }
    }
}
