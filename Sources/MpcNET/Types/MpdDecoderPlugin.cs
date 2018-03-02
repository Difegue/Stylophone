// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdDecoderPlugin.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a MPD decoder plugin.
    /// </summary>
    public class MpdDecoderPlugin
    {
        /// <summary>
        /// The empty plugiun.
        /// </summary>
        public static readonly MpdDecoderPlugin Empty = new MpdDecoderPlugin(string.Empty);

        private readonly List<string> suffixes = new List<string>();
        private readonly List<string> mediaTypes = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MpdDecoderPlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MpdDecoderPlugin(string name)
        {
            this.Name = name;
            this.IsInitialized = !string.IsNullOrEmpty(name);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the suffixes.
        /// </summary>
        /// <value>
        /// The suffixes.
        /// </value>
        public IReadOnlyList<string> Suffixes => this.suffixes;

        /// <summary>
        /// Gets the media types.
        /// </summary>
        /// <value>
        /// The media types.
        /// </value>
        public IReadOnlyList<string> MediaTypes => this.mediaTypes;

        internal bool IsInitialized { get; }

        internal void AddSuffix(string suffix)
        {
            this.suffixes.Add(suffix);
        }

        internal void AddMediaType(string type)
        {
            this.mediaTypes.Add(type);
        }
    }
}