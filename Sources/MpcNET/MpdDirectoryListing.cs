// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdDirectoryListing.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using System;
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// The MpdDirectoryListing class contains the response of a MPD server to a list command.
    /// </summary>
    public class MpdDirectoryListing
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdDirectoryListing" /> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="playlist">The playlist.</param>
        public MpdDirectoryListing(List<IMpdFile> file, List<string> directory, List<string> playlist)
        {
            this.FileListList = file ?? throw new ArgumentNullException("file");
            this.DirectoryList = directory ?? throw new ArgumentNullException("directory");
            this.PlaylistList = playlist ?? throw new ArgumentNullException("playlist");
        }

        /// <summary>
        /// Gets the list of files in the directory.
        /// </summary>
        public IReadOnlyList<IMpdFile> FileListList { get; }

        /// <summary>
        /// Gets the list of subdirectories in the directory.
        /// </summary>
        public IReadOnlyList<string> DirectoryList { get; }

        /// <summary>
        /// Gets the list of playlists in the directory.
        /// </summary>
        public IReadOnlyList<string> PlaylistList { get; }
    }
}
