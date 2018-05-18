// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdStatistics.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET
{
    using System.Text;

    /// <summary>
    /// The MpdStatistics class contains statistics of the MPD file database.
    /// </summary>
    public class MpdStatistics
    {
        private const string ArtistsText = "artists";
        private const string SongsText = "songs";
        private const string UptimeText = "uptime";
        private const string PlaytimeText = "playtime";
        private const string DbPlaytimeText = "db_playtime";
        private const string DbUpdateText = "db_update";

        /// <summary>
        /// Initializes a new instance of the <see cref="MpdStatistics"/> class.
        /// </summary>
        /// <param name="artists">The number of artists in the MPD database.</param>
        /// <param name="albums">The number of albums in the MPD database.</param>
        /// <param name="songs">The number of songs in the MPD database.</param>
        /// <param name="uptime">The time the MPD server is running in seconds.</param>
        /// <param name="playtime">The number of seconds the MPD played so far.</param>
        /// <param name="dbPlaytime">The total playtime of all songs in the MPD database.</param>
        /// <param name="dbUpdate">The timestamp of the last MPD database update.</param>
        public MpdStatistics(
            int artists,
            int albums,
            int songs,
            int uptime,
            int playtime,
            int dbPlaytime,
            long dbUpdate)
        {
            this.Artists = artists;
            this.Albums = albums;
            this.Songs = songs;
            this.Uptime = uptime;
            this.Playtime = playtime;
            this.DbPlaytime = dbPlaytime;
            this.DbUpdate = dbUpdate;
        }

        /// <summary>
        /// Gets the number of artists in the MPD database.
        /// </summary>
        public int Artists { get; }

        /// <summary>
        /// Gets the number of albums in the MPD database.
        /// </summary>
        public int Albums { get; }

        /// <summary>
        /// Gets the number of songs in the MPD database.
        /// </summary>
        public int Songs { get; }

        /// <summary>
        /// Gets the time the MPD server is running in seconds.
        /// </summary>
        public int Uptime { get; }

        /// <summary>
        /// Gets the number of seconds the MPD played so far.
        /// </summary>
        public int Playtime { get; }

        /// <summary>
        /// Gets the total playtime of all songs in the MPD database.
        /// </summary>
        public int DbPlaytime { get; }

        /// <summary>
        /// Gets the timestamp of the last MPD database update.
        /// </summary>
        public long DbUpdate { get; }

        /// <summary>
        /// Returns a string representation of the object mainly for debugging purpuse.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            AppendInt(builder, ArtistsText, this.Artists);
            AppendInt(builder, SongsText, this.Songs);
            AppendInt(builder, UptimeText, this.Uptime);
            AppendInt(builder, PlaytimeText, this.Playtime);
            AppendInt(builder, DbPlaytimeText, this.DbPlaytime);
            AppendLong(builder, DbUpdateText, this.DbUpdate);

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

        private static void AppendLong(StringBuilder builder, string name, long value)
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
    }
}
