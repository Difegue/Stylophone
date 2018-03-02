// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlaybackCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using MpcNET.Commands.Playback;
    using MpcNET.Types;

    /// <summary>
    /// https://www.musicpd.org/doc/protocol/playback_commands.html
    /// </summary>
    public class PlaybackCommandFactory : IPlaybackCommandFactory
    {
        /// <summary>
        /// Get a next command.
        /// </summary>
        /// <returns>A <see cref="NextCommand"/>.</returns>
        public IMpcCommand<string> Next()
        {
            return new NextCommand();
        }

        /// <summary>
        /// Get a previous command.
        /// </summary>
        /// <returns>A <see cref="StopCommand"/>.</returns>
        public IMpcCommand<string> Previous()
        {
            return new PreviousCommand();
        }

        /// <summary>
        /// Gets a play-pause command.
        /// </summary>
        /// <returns>A <see cref="PlayPauseCommand"/>.</returns>
        public IMpcCommand<string> PlayPause()
        {
            return new PlayPauseCommand();
        }

        /// <summary>
        /// Gets a play command.
        /// </summary>
        /// <param name="mpdFile">The MPD file.</param>
        /// <returns>A <see cref="PlayCommand"/>.</returns>
        public IMpcCommand<string> Play(IMpdFile mpdFile)
        {
            return new PlayCommand(mpdFile.Id, mpdFile.Position);
        }

        /// <summary>
        /// Gets a play command.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>A <see cref="PlayCommand"/>.</returns>
        public IMpcCommand<string> Play(int position)
        {
            return new PlayCommand(position, MpdFile.NoId);
        }

        /// <summary>
        /// Gets a play command.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="PlayCommand"/>.</returns>
        public IMpcCommand<string> PlayId(int id)
        {
            return new PlayCommand(MpdFile.NoPos, id);
        }

        /// <summary>
        /// Gets a stop command.
        /// </summary>
        /// <returns>A <see cref="StopCommand"/>.</returns>
        public IMpcCommand<string> Stop()
        {
            return new StopCommand();
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>
        /// A <see cref="T:MpcNET.Commands.Playback.SetVolumeCommand" />.
        /// </returns>
        public IMpcCommand<string> SetVolume(byte volume)
        {
            return new SetVolumeCommand(volume);
        }
    }
}
