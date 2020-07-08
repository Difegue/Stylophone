// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Text;

namespace MpcNET
{
    internal class Constants
    {
        public static readonly string Ok = "OK";

        public static readonly string Ack = "ACK";

        public static readonly string Binary = "binary: ";

        public static readonly string FirstLinePrefix = "OK MPD ";

        /// <summary>
        /// Encoding used when reading server responses.
        /// </summary>
        public static readonly Encoding Encoding = new UTF8Encoding();
    }
}
