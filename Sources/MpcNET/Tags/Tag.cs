// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tag.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Tags
{
    internal class Tag : ITag
    {
        internal Tag(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }
}