// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdState.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    /// <summary>
    /// The possible states of the MPD.
    /// </summary>
    public enum MpdState
    {
        /// <summary>
        /// The state of the MPD could not be translated into this enumeration.
        /// </summary>
        Unknown,

        /// <summary>
        /// The MPD is playing a track.
        /// </summary>
        Play,

        /// <summary>
        /// The MPD is not playing a track.
        /// </summary>
        Stop,

        /// <summary>
        /// The playback of the MPD is currently paused.
        /// </summary>
        Pause,
    }
}