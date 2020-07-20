// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMpdFile.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for representing MPD files.
    /// </summary>
    /// <seealso cref="IMpdFilePath" />
    public interface IMpdFile : IMpdFilePath
    {
        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        int Time { get; }

        /// <summary>
        /// Gets the album.
        /// </summary>
        /// <value>
        /// The album.
        /// </value>
        string Album { get; }

        /// <summary>
        /// Gets the artist.
        /// </summary>
        /// <value>
        /// The artist.
        /// </value>
        string Artist { get; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; }

        /// <summary>
        /// Gets the track.
        /// </summary>
        /// <value>
        /// The track.
        /// </value>
        string Track { get; }

        /// <summary>
        /// Gets the genre.
        /// </summary>
        /// <value>
        /// The genre.
        /// </value>
        string Genre { get; }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        string Date { get; }

        /// <summary>
        /// Gets the composer.
        /// </summary>
        /// <value>
        /// The composer.
        /// </value>
        string Composer { get; }

        /// <summary>
        /// Gets the performer.
        /// </summary>
        /// <value>
        /// The performer.
        /// </value>
        string Performer { get; }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        string Comment { get; }

        /// <summary>
        /// Gets the disc.
        /// </summary>
        /// <value>
        /// The disc.
        /// </value>
        int Disc { get; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        int Position { get; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; }

        /// <summary>
        /// Gets the unknown metadata.
        /// </summary>
        /// <value>
        /// The unknown metadata.
        /// </value>
        IReadOnlyDictionary<string, string> UnknownMetadata { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Time"/> property set.
        /// </summary>
        bool HasTime { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Album"/> property set.
        /// </summary>
        bool HasAlbum { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Artist"/> property set.
        /// </summary>
        bool HasArtist { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Title"/> property set.
        /// </summary>
        bool HasTitle { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Track"/> property set.
        /// </summary>
        bool HasTrack { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="IMpdFilePath.Name"/> property set.
        /// </summary>
        bool HasName { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Genre"/> property set.
        /// </summary>
        bool HasGenre { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Date"/> property set.
        /// </summary>
        bool HasDate { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Composer"/> property set.
        /// </summary>
        bool HasComposer { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Performer"/> property set.
        /// </summary>
        bool HasPerformer { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Comment"/> property set.
        /// </summary>
        bool HasComment { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Disc"/> property set.
        /// </summary>
        bool HasDisc { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Position"/> property set.
        /// </summary>
        bool HasPos { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Id"/> property set.
        /// </summary>
        bool HasId { get; }
    }
}