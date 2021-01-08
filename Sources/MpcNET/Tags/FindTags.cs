// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindTags.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Tags
{
    /// <summary>
    /// https://www.musicpd.org/doc/protocol/database.html : find {TYPE} {WHAT} [...] [window START:END].
    /// </summary>
    public class FindTags
    {
        /// <summary>
        /// Gets the any tag.
        /// </summary>
        /// <value>
        /// Any.
        /// </value>
        public static ITag Any { get; } = new Tag("any");

        /// <summary>
        /// Gets the file tag.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public static ITag File { get; } = new Tag("file");

        /// <summary>
        /// Gets the title tag.
        /// </summary>
        /// <value>
        /// The base.
        /// </value>
        public static ITag Title { get; } = new Tag("title");

        /// <summary>
        /// Gets the artist tag.
        /// </summary>
        /// <value>
        /// The modified since.
        /// </value>
        public static ITag Artist { get; } = new Tag("artist");

        /// <summary>
        /// Gets the album tag.
        /// </summary>
        /// <value>
        /// The base.
        /// </value>
        public static ITag Album { get; } = new Tag("album");
    }
}