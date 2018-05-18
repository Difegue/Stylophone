// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpcException.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Exceptions
{
    using System;

    /// <summary>
    /// Base class for all exceptions.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class MpcException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpcException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MpcException(string message)
            : base(message)
        {
        }
    }
}