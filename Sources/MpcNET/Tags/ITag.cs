// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITag.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Tags
{
    /// <summary>
    /// Interface for representing a tag.
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        string Value { get; }
    }
}