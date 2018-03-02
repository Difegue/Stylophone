// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdResponse.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    /// <summary>
    /// Represents response to a <see cref="IMpcCommand{TValue}"/>.
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    public interface IMpdResponse<TContent>
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        IMpdResponseResult Result { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        TContent Content { get; }
    }
}