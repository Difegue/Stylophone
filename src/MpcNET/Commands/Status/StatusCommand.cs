using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MpcNET.Commands.Status
{
    internal class StatusCommand : IMpcCommand<MpdStatus>
    {
        public string Value => "status";

        public MpdStatus FormatResponse(IList<KeyValuePair<string, string>> response)
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
            int updatingDb = -1;
            string error = string.Empty;
            foreach (var keyValuePair in response)
            {
                var value = keyValuePair.Value;
                switch (keyValuePair.Key)
                {
                    case "volume":
                        int.TryParse(value, out volume);
                        break;
                    case "repeat":
                        repeat = "1" == value;
                        break;
                    case "random":
                        random = "1" == value;
                        break;
                    case "single":
                        single = "1" == value;
                        break;
                    case "consume":
                        consume = "1" == value;
                        break;
                    case "playlist":
                        int.TryParse(value, out playlist);
                        break;
                    case "playlistlength":
                        int.TryParse(value, out playlistLength);
                        break;
                    case "song":
                        int.TryParse(value, out playlistSong);
                        break;
                    case "songid":
                        int.TryParse(value, out playlistSongId);
                        break;
                    case "nextsong":
                        int.TryParse(value, out playlistNextSong);
                        break;
                    case "nextsongid":
                        int.TryParse(value, out playlistNextSongId);
                        break;
                    case "bitrate":
                        int.TryParse(value, out bitrate);
                        break;
                    case "audio":
                        var audioFormat = value.Split(':');
                        int.TryParse(audioFormat[0], out audioSampleRate);
                        int.TryParse(audioFormat[1], out audioBits);
                        int.TryParse(audioFormat[2], out audioChannels);
                        break;
                    case "xfade":
                        int.TryParse(value, out crossfade);
                        break;
                    case "state":
                        Enum.TryParse(value, true, out mpdState);
                        break;
                    case "elapsed":
                        elapsed = ParseTime(value);
                        break;
                    case "duration":
                        duration = ParseTime(value);
                        break;
                    case "updating_db":
                        int.TryParse(value, out updatingDb);
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
