// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandNullException.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="MpcConnection"/> when the command is null.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CommandNullException : MpcException
    {
        internal CommandNullException()
            : base("No command was specified")
        {
        }
    }
}