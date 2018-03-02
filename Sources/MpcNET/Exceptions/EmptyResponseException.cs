// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmptyResponseException.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Exceptions
{
    using System;

    /// <summary>
    /// Exception throw when an empty response is received.
    /// </summary>
    /// <seealso cref="Exception" />
    public class EmptyResponseException : MpcException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResponseException"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        public EmptyResponseException(string command)
            : base($"The command: {command} returned no response")
        {
        }
    }
}