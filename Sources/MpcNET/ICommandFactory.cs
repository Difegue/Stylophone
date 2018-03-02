// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using MpcNET.Commands;

    /// <summary>
    /// Interface for providing specific command factories.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Gets the status command factory.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        IStatusCommandFactory Status { get; }

        /// <summary>
        /// Gets the database command factory.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        IDatabaseCommandFactory Database { get; }

        /// <summary>
        /// Gets the reflection command factory.
        /// </summary>
        /// <value>
        /// The reflection.
        /// </value>
        IReflectionCommandFactory Reflection { get; }

        /// <summary>
        /// Gets the stored playlist command factory.
        /// </summary>
        /// <value>
        /// The stored playlist.
        /// </value>
        IStoredPlaylistCommandFactory StoredPlaylist { get; }

        /// <summary>
        /// Gets the current playlist command factory.
        /// </summary>
        /// <value>
        /// The current playlist.
        /// </value>
        ICurrentPlaylistCommandFactory CurrentPlaylist { get; }

        /// <summary>
        /// Gets the playback command factory.
        /// </summary>
        /// <value>
        /// The playback.
        /// </value>
        IPlaybackCommandFactory Playback { get; }

        /// <summary>
        /// Gets the output command factory.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        IOutputCommandFactory Output { get; }
    }
}