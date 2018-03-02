// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using MpcNET.Commands.Status;
    using MpcNET.Types;

    /// <summary>
    /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands
    /// </summary>
    public class StatusCommandFactory : IStatusCommandFactory
    {
        /// <summary>
        /// Gets a status command.
        /// </summary>
        /// <returns>A <see cref="StatusCommand"/>.</returns>
        public IMpcCommand<MpdStatus> GetStatus()
        {
            return new StatusCommand();
        }

        /// <summary>
        /// Gets a current song command.
        /// </summary>
        /// <returns>A <see cref="CurrentSongCommand"/>.</returns>
        public IMpcCommand<IMpdFile> GetCurrentSong()
        {
            return new CurrentSongCommand();
        }
    }
}
