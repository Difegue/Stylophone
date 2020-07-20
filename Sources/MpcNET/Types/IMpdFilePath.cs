// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdFilePath.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    /// <summary>
    /// Interface for representing a MPD file path.
    /// </summary>
    public interface IMpdFilePath
    {
        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        string Path { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }
    }
}