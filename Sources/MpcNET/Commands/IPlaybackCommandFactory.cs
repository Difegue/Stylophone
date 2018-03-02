// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlaybackCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using MpcNET.Commands.Playback;
    using MpcNET.Types;

    /// <summary>
    /// Provides playback commands.
    /// </summary>
    public interface IPlaybackCommandFactory
    {
        /// <summary>
        /// Get a next command.
        /// </summary>
        /// <returns>A <see cref="NextCommand"/>.</returns>
        IMpcCommand<string> Next();

        /// <summary>
        /// Get a previous command.
        /// </summary>
        /// <returns>A <see cref="StopCommand"/>.</returns>
        IMpcCommand<string> Previous();

        /// <summary>
        /// Gets a play-pause command.
        /// </summary>
        /// <returns>A <see cref="PlayPauseCommand"/>.</returns>
        IMpcCommand<string> PlayPause();

        /// <summary>
        /// Gets a play command.
        /// </summary>
        /// <param name="mpdFile">The MPD file.</param>
        /// <returns>A <see cref="PlayCommand"/>.</returns>
        IMpcCommand<string> Play(IMpdFile mpdFile);

        /// <summary>
        /// Gets a play command.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>A <see cref="PlayCommand"/>.</returns>
        IMpcCommand<string> Play(int position);

        /// <summary>
        /// Gets a play command.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="PlayCommand"/>.</returns>
        IMpcCommand<string> PlayId(int id);

        /// <summary>
        /// Gets a stop command.
        /// </summary>
        /// <returns>A <see cref="StopCommand"/>.</returns>
        IMpcCommand<string> Stop();

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>
        /// A <see cref="SetVolumeCommand" />.
        /// </returns>
        IMpcCommand<string> SetVolume(byte volume);
    }
}