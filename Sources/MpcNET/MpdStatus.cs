// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdStatus.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using System;
    using System.Text;

    /// <summary>
    /// The MpdStatus class contains all values describing the current status of the MPD.
    /// </summary>
    public class MpdStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdStatus" /> class.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <param name="repeat">if set to <c>true</c> [repeat].</param>
        /// <param name="random">if set to <c>true</c> [random].</param>
        /// <param name="consume">if set to <c>true</c> [consume].</param>
        /// <param name="single">if set to <c>true</c> [single].</param>
        /// <param name="playlist">The playlist.</param>
        /// <param name="playlistLength">Length of the playlist.</param>
        /// <param name="xFade">The x fade.</param>
        /// <param name="state">The state.</param>
        /// <param name="song">The song.</param>
        /// <param name="songId">The song identifier.</param>
        /// <param name="nextSong">The next song.</param>
        /// <param name="nextSongId">The next song identifier.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="bitrate">The bitrate.</param>
        /// <param name="audioSampleRate">The audio sample rate.</param>
        /// <param name="audioBits">The audio bits.</param>
        /// <param name="audioChannels">The audio channels.</param>
        /// <param name="updatingDb">The updating database.</param>
        /// <param name="partition">The partition name.</param>
        /// <param name="error">The error.</param>
        public MpdStatus(
            int volume,
            bool repeat,
            bool random,
            bool consume,
            bool single,
            int playlist,
            int playlistLength,
            int xFade,
            MpdState state,
            int song,
            int songId,
            int nextSong,
            int nextSongId,
            TimeSpan elapsed,
            TimeSpan duration,
            int bitrate,
            int audioSampleRate,
            int audioBits,
            int audioChannels,
            int updatingDb,
            string partition,
            string error)
        {
            Volume = volume;
            Repeat = repeat;
            Random = random;
            Consume = consume;
            Single = single;
            Playlist = playlist;
            PlaylistLength = playlistLength;
            XFade = xFade;
            State = state;
            Song = song;
            SongId = songId;
            NextSong = nextSong;
            NextSongId = nextSongId;
            Elapsed = elapsed;
            Duration = duration;
            Bitrate = bitrate;
            AudioSampleRate = audioSampleRate;
            AudioBits = audioBits;
            AudioChannels = audioChannels;
            UpdatingDb = updatingDb;
            Partition = partition;
            Error = error;
        }

        /// <summary>
        /// Gets the current volume of the output.
        /// </summary>
        public int Volume { get; }

        /// <summary>
        /// Gets a value indicating whether the playlist is repeated after finish.
        /// </summary>
        public bool Repeat { get; }

        /// <summary>
        /// Gets a value indicating whether the playlist is played in random order.
        /// </summary>
        public bool Random { get; }

        /// <summary>
        /// Gets a value indicating whether the playlist is consumed.
        /// </summary>
        public bool Consume { get; }

        /// <summary>
        /// Gets a value indicating whether the playlist only plays a song once when random is enabled.
        /// </summary>
        public bool Single { get; }

        /// <summary>
        /// Gets the version number of the playlist.
        /// </summary>
        public int Playlist { get; }

        /// <summary>
        /// Gets the length of the playlist.
        /// </summary>
        public int PlaylistLength { get; }

        /// <summary>
        /// Gets the number of seconds crossfaded between song changes.
        /// </summary>
        public int XFade { get; }

        /// <summary>
        /// Gets the state of the MPD.
        /// </summary>
        public MpdState State { get; }

        /// <summary>
        /// Gets the index of the currently played song in the playlist.
        /// </summary>
        public int Song { get; }

        /// <summary>
        /// Gets the id of the song currently played.
        /// </summary>
        public int SongId { get; }

        /// <summary>
        /// Gets the next song.
        /// </summary>
        public int NextSong { get; }

        /// <summary>
        /// Gets the next song identifier.
        /// </summary>
        public int NextSongId { get; }

        /// <summary>
        /// Gets the number of seconds already played of the current song.
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Gets the length of the current song in seconds.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the bitrate of the current song.
        /// </summary>
        public int Bitrate { get; }

        /// <summary>
        /// Gets the audio sample rate of the current song.
        /// </summary>
        public int AudioSampleRate { get; }

        /// <summary>
        /// Gets the audio bits of the current song.
        /// </summary>
        public int AudioBits { get; }

        /// <summary>
        /// Gets the number of audio channels of the current song.
        /// </summary>
        public int AudioChannels { get; }

        /// <summary>
        /// Gets the number of the update on the MPD database currently running.
        /// </summary>
        public int UpdatingDb { get; }

        /// <summary>
        /// Gets the name of the current partition
        /// </summary>
        public string Partition { get; }


        /// <summary>
        /// Gets the error message, if there is an error.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Returns a string representation of the object maily for debugging purpuses.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"partition: {Partition}");
            AppendInt(builder, "volume", Volume);
            AppendBool(builder, "repeat", Repeat);
            AppendBool(builder, "random", Random);
            AppendInt(builder, "playlist", Playlist);
            AppendInt(builder, "playlistlength", PlaylistLength);
            AppendInt(builder, "xfade", XFade);
            switch (State)
            {
                case MpdState.Play:
                    builder.AppendLine("state: play");
                    break;
                case MpdState.Pause:
                    builder.AppendLine("state: pause");
                    break;
                case MpdState.Stop:
                    builder.AppendLine("state: stop");
                    break;
            }

            AppendInt(builder, "song", Song);
            AppendInt(builder, "songid", SongId);
            if (Elapsed > TimeSpan.Zero || Duration > TimeSpan.Zero)
            {
                builder.Append("time: ");
                builder.Append(Elapsed);
                builder.Append(":");
                builder.Append(Duration);
                builder.AppendLine();
            }

            AppendInt(builder, "bitrate", Bitrate);
            if ((AudioSampleRate >= 0) || (AudioBits >= 0) || (AudioChannels >= 0))
            {
                builder.Append("audio: ");
                builder.Append(AudioSampleRate);
                builder.Append(":");
                builder.Append(AudioBits);
                builder.Append(":");
                builder.Append(AudioChannels);
                builder.AppendLine();
            }

            AppendInt(builder, "updating_db", UpdatingDb);
            if (Error != null)
            {
                builder.Append("error: ");
                builder.AppendLine(Error);
            }

            return builder.ToString();
        }

        private static void AppendInt(StringBuilder builder, string name, int value)
        {
            if (value < 0)
            {
                return;
            }

            builder.Append(name);
            builder.Append(": ");
            builder.Append(value);
            builder.AppendLine();
        }

        private static void AppendBool(StringBuilder builder, string name, bool value)
        {
            builder.Append(name);
            builder.Append(": ");
            builder.Append(value ? '1' : '0');
            builder.AppendLine();
        }
    }
}
