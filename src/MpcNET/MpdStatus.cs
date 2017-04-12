using System.Text;

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
        Pause
    }
    /// <summary>
    /// The MpdStatus class contains all values describing the current status of the MPD.
    /// </summary>
    public class MpdStatus
    {
        private int volume;
        private bool repeat;
        private bool random;
        private int playlist;
        private int playlistLength;
        private int xFade;
        private MpdState state;
        private int song;
        private int songId;
        private int timeElapsed;
        private int timeTotal;
        private int bitrate;
        private int audioSampleRate;
        private int audioBits;
        private int audioChannels;
        private int updatingDb;
        private string error;
        /// <summary>
        /// The current volume of the output.
        /// </summary>
        public int Volume { get { return this.volume; } }
        /// <summary>
        /// If the playlist is repeated after finish.
        /// </summary>
        public bool Repeat { get { return this.repeat; } }
        /// <summary>
        /// If the playlist is played in random order.
        /// </summary>
        public bool Random { get { return this.random; } }
        /// <summary>
        /// The version number of the playlist.
        /// </summary>
        public int Playlist { get { return this.playlist; } }
        /// <summary>
        /// The length of the playlist.
        /// </summary>
        public int PlaylistLength { get { return this.playlistLength; } }
        /// <summary>
        /// The number of seconds crossfaded between song changes.
        /// </summary>
        public int XFade { get { return this.xFade; } }
        /// <summary>
        /// The state of the MPD.
        /// </summary>
        public MpdState State { get { return this.state; } }
        /// <summary>
        /// The index of the currently played song in the playlist.
        /// </summary>
        public int Song { get { return this.song; } }
        /// <summary>
        /// The id of the song currently played.
        /// </summary>
        public int SongId { get { return this.songId; } }
        /// <summary>
        /// The number of seconds already played of the current song.
        /// </summary>
        public int TimeElapsed { get { return this.timeElapsed; } }
        /// <summary>
        /// The length of the current song in seconds.
        /// </summary>
        public int TimeTotal { get { return this.timeTotal; } }
        /// <summary>
        /// The bitrate of the current song.
        /// </summary>
        public int Bitrate { get { return this.bitrate; } }
        /// <summary>
        /// The audio sample rate of the current song.
        /// </summary>
        public int AudioSampleRate { get { return this.audioSampleRate; } }
        /// <summary>
        /// The audio bits of the current song.
        /// </summary>
        public int AudioBits { get { return this.audioBits; } }
        /// <summary>
        /// The number of audio channels of the current song.
        /// </summary>
        public int AudioChannels { get { return this.audioChannels; } }
        /// <summary>
        /// The number of the update on the MPD database currently running.
        /// </summary>
        public int UpdatingDb { get { return this.updatingDb; } }
        /// <summary>
        /// An error message, if there is an error.
        /// </summary>
        public string Error { get { return this.error; } }
        /// <summary>
        /// Creates a new MpdStatus object.
        /// </summary>
        /// <param name="volume">The current volume of the output.</param>
        /// <param name="repeat">If the playlist is repeated after finish.</param>
        /// <param name="random">If the playlist is played in random order.</param>
        /// <param name="playlist">The version number of the playlist.</param>
        /// <param name="playlistLength">The length of the playlist.</param>
        /// <param name="xFade">The number of seconds crossfaded between song changes.</param>
        /// <param name="state">The state of the MPD.</param>
        /// <param name="song">The index of the currently played song in the playlist.</param>
        /// <param name="songId">The id of the song currently played.</param>
        /// <param name="timeElapsed">The number of seconds already played of the current song.</param>
        /// <param name="timeTotal">The length of the current song in seconds.</param>
        /// <param name="bitrate">The bitrate of the current song.</param>
        /// <param name="audioSampleRate">The audio sample rate of the current song.</param>
        /// <param name="audioBits">The audio bits of the current song.</param>
        /// <param name="audioChannels">The number of audio channels of the current song.</param>
        /// <param name="updatingDb">The number of the update on the MPD database currently running.</param>
        /// <param name="error">An error message, if there is an error.</param>
        public MpdStatus(
            int volume,
            bool repeat,
            bool random,
            int playlist,
            int playlistLength,
            int xFade,
            MpdState state,
            int song,
            int songId,
            int timeElapsed,
            int timeTotal,
            int bitrate,
            int audioSampleRate,
            int audioBits,
            int audioChannels,
            int updatingDb,
            string error
            )
        {
            this.volume = volume;
            this.repeat = repeat;
            this.random = random;
            this.playlist = playlist;
            this.playlistLength = playlistLength;
            this.xFade = xFade;
            this.state = state;
            this.song = song;
            this.songId = songId;
            this.timeElapsed = timeElapsed;
            this.timeTotal = timeTotal;
            this.bitrate = bitrate;
            this.audioSampleRate = audioSampleRate;
            this.audioBits = audioBits;
            this.audioChannels = audioChannels;
            this.updatingDb = updatingDb;
            this.error = error;
        }
        /// <summary>
        /// Returns a string representation of the object maily for debugging purpuses.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            appendInt(builder, "volume", this.volume);
            appendBool(builder, "repeat", this.repeat);
            appendBool(builder, "random", this.random);
            appendInt(builder, "playlist", this.playlist);
            appendInt(builder, "playlistlength", this.playlistLength);
            appendInt(builder, "xfade", this.xFade);
            switch (this.state)
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
            appendInt(builder, "song", this.song);
            appendInt(builder, "songid", this.songId);
            if ((this.timeElapsed >= 0) || (this.timeTotal >= 0))
            {
                builder.Append("time: ");
                builder.Append(this.timeElapsed);
                builder.Append(":");
                builder.Append(this.timeTotal);
                builder.AppendLine();
            }
            appendInt(builder, "bitrate", this.bitrate);
            if ((this.audioSampleRate >= 0) || (this.audioBits >= 0) || (this.audioChannels >= 0))
            {
                builder.Append("audio: ");
                builder.Append(this.audioSampleRate);
                builder.Append(":");
                builder.Append(this.audioBits);
                builder.Append(":");
                builder.Append(this.audioChannels);
                builder.AppendLine();
            }
            appendInt(builder, "updating_db", this.updatingDb);
            if (this.error != null)
            {
                builder.Append("error: ");
                builder.AppendLine(this.error);
            }

            return builder.ToString();
        }

        private static void appendInt(StringBuilder builder, string name, int value)
        {
            if (value < 0)
                return;

            builder.Append(name);
            builder.Append(": ");
            builder.Append(value);
            builder.AppendLine();
        }

        private static void appendBool(StringBuilder builder, string name, bool value)
        {
            builder.Append(name);
            builder.Append(": ");
            builder.Append(value ? '1' : '0');
            builder.AppendLine();
        }
    }
}
