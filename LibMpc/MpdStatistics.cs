using System.Text;

namespace LibMpc
{
    /// <summary>
    /// The MpdStatistics class contains statistics of the MPD file database.
    /// </summary>
    public class MpdStatistics
    {
        private readonly int artists;
        private readonly int albums;
        private readonly int songs;
        private readonly int uptime;
        private readonly int playtime;
        private readonly int db_playtime;
        private readonly long db_update;
        /// <summary>
        /// The number of artists in the MPD database.
        /// </summary>
        public int Artists { get { return this.artists; } }
        /// <summary>
        /// The number of albums in the MPD database.
        /// </summary>
        public int Albums { get { return this.albums; } }
        /// <summary>
        /// The number of songs in the MPD database.
        /// </summary>
        public int Songs { get { return this.songs; } }
        /// <summary>
        /// The time the MPD server is running in seconds.
        /// </summary>
        public int Uptime { get { return this.uptime; } }
        /// <summary>
        /// The number of seconds the MPD played so far.
        /// </summary>
        public int Playtime { get { return this.playtime; } }
        /// <summary>
        /// The total playtime of all songs in the MPD database.
        /// </summary>
        public int DbPlaytime { get { return this.db_playtime; } }
        /// <summary>
        /// The timestamp of the last MPD database update.
        /// </summary>
        public long DbUpdate { get { return this.db_update; } }
        /// <summary>
        /// Creates a new MpdStatistics object.
        /// </summary>
        /// <param name="artists">The number of artists in the MPD database.</param>
        /// <param name="albums">The number of albums in the MPD database.</param>
        /// <param name="songs">The number of songs in the MPD database.</param>
        /// <param name="uptime">The time the MPD server is running in seconds.</param>
        /// <param name="playtime">The number of seconds the MPD played so far.</param>
        /// <param name="db_playtime">The total playtime of all songs in the MPD database.</param>
        /// <param name="db_update">The timestamp of the last MPD database update.</param>
        public MpdStatistics(
            int artists,
            int albums,
            int songs,
            int uptime,
            int playtime,
            int db_playtime,
            long db_update
            )
        {
            this.artists = artists;
            this.albums = albums;
            this.songs = songs;
            this.uptime = uptime;
            this.playtime = playtime;
            this.db_playtime = db_playtime;
            this.db_update = db_update;
        }
        /// <summary>
        /// Returns a string representation of the object mainly for debugging purpuse.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            appendInt(builder, "artists", this.artists);
            appendInt(builder, "songs", this.songs);
            appendInt(builder, "uptime", this.uptime);
            appendInt(builder, "playtime", this.playtime);
            appendInt(builder, "db_playtime", this.db_playtime);
            appendLong(builder, "db_update", this.db_update);

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

        private static void appendLong(StringBuilder builder, string name, long value)
        {
            if (value < 0)
                return;

            builder.Append(name);
            builder.Append(": ");
            builder.Append(value);
            builder.AppendLine();
        }

    }
}
