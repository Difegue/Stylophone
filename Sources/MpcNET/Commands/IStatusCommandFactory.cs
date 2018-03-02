// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStatusCommandFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands
{
    using MpcNET.Commands.Status;
    using MpcNET.Types;

    /// <summary>
    /// Provides status commands.
    /// </summary>
    public interface IStatusCommandFactory
    {
        /// <summary>
        /// Gets a status command.
        /// </summary>
        /// <returns>A <see cref="StatusCommand"/>.</returns>
        IMpcCommand<MpdStatus> GetStatus();

        /// <summary>
        /// Gets a current song command.
        /// </summary>
        /// <returns>A <see cref="CurrentSongCommand"/>.</returns>
        IMpcCommand<IMpdFile> GetCurrentSong();
    }
}