// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Status
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Gets the status.
    /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands.
    /// </summary>
    public class StatusCommand : IMpcCommand<MpdStatus>
    {
        private const string VolumeText = "volume";
        private const string RepeatText = "repeat";
        private const string RandomText = "random";
        private const string SingleText = "single";
        private const string ConsumeText = "consume";
        private const string PlaylistText = "playlist";
        private const string PlaylistlengthText = "playlistlength";
        private const string SongText = "song";
        private const string SongidText = "songid";
        private const string NextsongText = "nextsong";
        private const string NextsongidText = "nextsongid";
        private const string BitrateText = "bitrate";
        private const string AudioText = "audio";
        private const string XfadeText = "xfade";
        private const string StateText = "state";
        private const string TimeText = "time";
        private const string ElapsedText = "elapsed";
        private const string DurationText = "duration";
        private const string MixrampDbText = "mixrampdb";
        private const string UpdatingDbText = "updating_db";
        private const string PartitionText = "partition";

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "status";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public MpdStatus Deserialize(SerializedResponse response)
        {
            int volume = -1;
            bool repeat = false;
            bool random = false;
            bool single = false;
            bool consume = false;
            int playlist = -1;
            int playlistLength = 0;
            int playlistSong = -1;
            int playlistSongId = -1;
            int playlistNextSong = -1;
            int playlistNextSongId = -1;
            int bitrate = 0;
            int audioSampleRate = -1;
            int audioBits = -1;
            int audioChannels = -1;
            int crossfade = -1;
            MpdState mpdState = MpdState.Unknown;
            TimeSpan elapsed;
            TimeSpan duration;
            double mixrampDb = -1;
            int updatingDb = -1;
            string partition = string.Empty;
            string error = string.Empty;
            foreach (var keyValuePair in response.ResponseValues)
            {
                var value = keyValuePair.Value;
                switch (keyValuePair.Key)
                {
                    case VolumeText:
                        int.TryParse(value, out volume);
                        break;
                    case RepeatText:
                        repeat = value == "1";
                        break;
                    case RandomText:
                        random = value == "1";
                        break;
                    case SingleText:
                        single = value == "1";
                        break;
                    case ConsumeText:
                        consume = value == "1";
                        break;
                    case PlaylistText:
                        int.TryParse(value, out playlist);
                        break;
                    case PlaylistlengthText:
                        int.TryParse(value, out playlistLength);
                        break;
                    case SongText:
                        int.TryParse(value, out playlistSong);
                        break;
                    case SongidText:
                        int.TryParse(value, out playlistSongId);
                        break;
                    case NextsongText:
                        int.TryParse(value, out playlistNextSong);
                        break;
                    case NextsongidText:
                        int.TryParse(value, out playlistNextSongId);
                        break;
                    case BitrateText:
                        int.TryParse(value, out bitrate);
                        break;
                    case AudioText:
                        var audioFormat = value.Split(':');
                        int.TryParse(audioFormat[0], out audioSampleRate);
                        int.TryParse(audioFormat[1], out audioBits);
                        int.TryParse(audioFormat[2], out audioChannels);
                        break;
                    case XfadeText:
                        int.TryParse(value, out crossfade);
                        break;
                    case StateText:
                        Enum.TryParse(value, true, out mpdState);
                        break;
                    case ElapsedText:
                        elapsed = ParseTime(value);
                        break;
                    case TimeText:
                        break;
                    case DurationText:
                        duration = ParseTime(value);
                        break;
                    case MixrampDbText:
                        double.TryParse(value, out mixrampDb);
                        break;
                    case UpdatingDbText:
                        int.TryParse(value, out updatingDb);
                        break;
                    case PartitionText:
                        partition = value;
                        break;
                    default:
                        Debug.WriteLine($"Unprocessed status: {keyValuePair.Key} - {keyValuePair.Value}");
                        break;
                }
            }

            return new MpdStatus(
                volume,
                repeat,
                random,
                consume,
                single,
                playlist,
                playlistLength,
                crossfade,
                mpdState,
                playlistSong,
                playlistSongId,
                playlistNextSong,
                playlistNextSongId,
                elapsed,
                duration,
                bitrate,
                audioSampleRate,
                audioBits,
                audioChannels,
                updatingDb,
                partition,
                error);
        }

        private static TimeSpan ParseTime(string value)
        {
            var timeParts = value.Split(new[] { '.' }, 2);
            int.TryParse(timeParts[0], out var seconds);
            int.TryParse(timeParts[1], out var milliseconds);
            return TimeSpan.FromSeconds(seconds) + TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}
