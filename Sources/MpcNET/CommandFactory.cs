// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandFactory.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MpcNET
{
    using MpcNET.Commands;

    /// <summary>
    /// Provides MPD commands.
    /// </summary>
    /// <seealso cref="MpcNET.ICommandFactory" />
    public class CommandFactory : ICommandFactory
    {
        /// <summary>
        /// Gets the status command factory.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public IStatusCommandFactory Status { get; } = new StatusCommandFactory();

        /// <summary>
        /// Gets the database command factory.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public IDatabaseCommandFactory Database { get; } = new DatabaseCommandFactory();

        /// <summary>
        /// Gets the reflection command factory.
        /// </summary>
        /// <value>
        /// The reflection.
        /// </value>
        public IReflectionCommandFactory Reflection { get; } = new ReflectionCommandFactory();

        /// <summary>
        /// Gets the stored playlist command factory.
        /// </summary>
        /// <value>
        /// The stored playlist.
        /// </value>
        public IStoredPlaylistCommandFactory StoredPlaylist { get; } = new StoredPlaylistCommandFactory();

        /// <summary>
        /// Gets the current playlist command factory.
        /// </summary>
        /// <value>
        /// The current playlist.
        /// </value>
        public ICurrentPlaylistCommandFactory CurrentPlaylist { get; } = new CurrentPlaylistCommandFactory();

        /// <summary>
        /// Gets the playback command factory.
        /// </summary>
        /// <value>
        /// The playback.
        /// </value>
        public IPlaybackCommandFactory Playback { get; } = new PlaybackCommandFactory();

        /// <summary>
        /// Gets the output command factory.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public IOutputCommandFactory Output { get; } = new OutputCommandFactory();
    }
}