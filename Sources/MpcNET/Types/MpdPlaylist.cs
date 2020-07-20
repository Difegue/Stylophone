// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdPlaylist.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is MpdPlaylist playlist &&
                   Name == playlist.Name &&
                   LastModified == playlist.LastModified;
        }
        /// <summary>
        /// HashCode implementation.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = 1509980188;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + LastModified.GetHashCode();
            return hashCode;
        }

        internal void AddLastModified(string lastModified)
        {
            this.LastModified = DateTime.Parse(lastModified, CultureInfo.InvariantCulture);
        }
    }
}
