// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdTags.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Tags
{
    /// <summary>
    /// https://www.musicpd.org/doc/protocol/tags.html.
    /// </summary>
    public class MpdTags
    {
        /// <summary>
        /// Gets the artist.
        /// </summary>
        /// <value>
        /// The artist.
        /// </value>
        public static ITag Artist { get; } = new Tag("artist");

        /// <summary>
        /// Gets the artist sort.
        /// </summary>
        /// <value>
        /// The artist sort.
        /// </value>
        public static ITag ArtistSort { get; } = new Tag("artistsort");

        /// <summary>
        /// Gets the album.
        /// </summary>
        /// <value>
        /// The album.
        /// </value>
        public static ITag Album { get; } = new Tag("album");

        /// <summary>
        /// Gets the album sort.
        /// </summary>
        /// <value>
        /// The album sort.
        /// </value>
        public static ITag AlbumSort { get; } = new Tag("albumsort");

        /// <summary>
        /// Gets the album artist.
        /// </summary>
        /// <value>
        /// The album artist.
        /// </value>
        public static ITag AlbumArtist { get; } = new Tag("albumartist");

        /// <summary>
        /// Gets the album artist sort.
        /// </summary>
        /// <value>
        /// The album artist sort.
        /// </value>
        public static ITag AlbumArtistSort { get; } = new Tag("albumartistsort");

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public static ITag Title { get; } = new Tag("title");

        /// <summary>
        /// Gets the track.
        /// </summary>
        /// <value>
        /// The track.
        /// </value>
        public static ITag Track { get; } = new Tag("track");

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public static ITag Name { get; } = new Tag("name");

        /// <summary>
        /// Gets the genre.
        /// </summary>
        /// <value>
        /// The genre.
        /// </value>
        public static ITag Genre { get; } = new Tag("genre");

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public static ITag Date { get; } = new Tag("date");

        /// <summary>
        /// Gets the composer.
        /// </summary>
        /// <value>
        /// The composer.
        /// </value>
        public static ITag Composer { get; } = new Tag("composer");

        /// <summary>
        /// Gets the performer.
        /// </summary>
        /// <value>
        /// The performer.
        /// </value>
        public static ITag Performer { get; } = new Tag("performer");

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public static ITag Comment { get; } = new Tag("comment");

        /// <summary>
        /// Gets the disc.
        /// </summary>
        /// <value>
        /// The disc.
        /// </value>
        public static ITag Disc { get; } = new Tag("disc");
    }
}