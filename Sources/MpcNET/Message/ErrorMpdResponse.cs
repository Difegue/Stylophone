// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorMpdResponse.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    using System;

    /// <summary>
    /// Implementation of <see cref="IMpdResponse{TContent}"/> in case of an error.
    /// </summary>
    /// <typeparam name="TContent">The content type.</typeparam>
    /// <seealso cref="MpcNET.Message.IMpdResponse{T}" />
    public class ErrorMpdResponse<TContent> : IMpdResponse<TContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMpdResponse{TContent}"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ErrorMpdResponse(Exception exception)
        {
            this.Result = new MpdResponseResult(null, false, exception);
            this.Content = default(TContent);
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