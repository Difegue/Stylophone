// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdDirectory.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a MPD directory.
    /// </summary>
    public class MpdDirectory: IMpdFilePath
    {
        private readonly List<IMpdFilePath> files = new List<IMpdFilePath>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MpdDirectory"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public MpdDirectory(string path)
        {
            this.Path = path;

            var name = path.Split('/').Last();
            this.Name = string.IsNullOrEmpty(name) ? "root" : name;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public IReadOnlyList<IMpdFilePath> Files => this.files;

        internal void AddFile(string file)
        {
            this.files.Add(new MpdFile(file));
        }
    }
}