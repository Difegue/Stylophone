// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdFilePath.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
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
    }
}