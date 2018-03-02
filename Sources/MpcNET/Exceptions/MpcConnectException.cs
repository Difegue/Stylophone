// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpcConnectException.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Exceptions
{
    using System;

    /// <summary>
    /// Exception thrown when there are problems with the <see cref="MpcConnection"/>.
    /// </summary>
    /// <seealso cref="Exception" />
    public class MpcConnectException : MpcException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpcConnectException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MpcConnectException(string message)
            : base(message)
        {
        }
    }
}