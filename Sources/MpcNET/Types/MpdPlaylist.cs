// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdPlaylist.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents a MPD playlist.
    /// </summary>
    public class MpdPlaylist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdPlaylist"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MpdPlaylist(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the last modified.
        /// </summary>
        /// <value>
        /// The last modified.
        /// </value>
        public DateTime LastModified { get; private set; }

        internal void AddLastModified(string lastModified)
        {
            this.LastModified = DateTime.Parse(lastModified, CultureInfo.InvariantCulture);
        }
    }
}
