/*
 * Copyright 2008 Matthias Sessler
 * 
 * This file is part of LibMpc.net.
 *
 * LibMpc.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * LibMpc.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with LibMpc.net.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Libmpc
{
    /// <summary>
    /// The MpdFile class contains all meta data for a file of the MPD.
    /// </summary>
    public class MpdFile
    {
        private const string TAG_FILE = "file";
        private const string TAG_TIME = "Time";
        private const string TAG_ARTIST = "Artist";
        private const string TAG_ALBUM = "Album";
        private const string TAG_TITLE = "Title";
        private const string TAG_TRACK = "Track";
        private const string TAG_NAME = "Name";
        private const string TAG_GENRE = "Genre";
        private const string TAG_DATE = "Date";
        private const string TAG_COMPOSER = "Composer";
        private const string TAG_PERFORMER = "Performer";
        private const string TAG_COMMENT = "Comment";
        private const string TAG_DISC = "Disc";
        private const string TAG_POS = "Pos";
        private const string TAG_ID = "Id";

        private const int NO_TIME = -1;
        private const string NO_ALBUM = null;
        private const string NO_ARTIST = null;
        private const string NO_TITLE = null;
        private const string NO_TRACK = null;
        private const string NO_NAME = null;
        private const string NO_GENRE = null;
        private const string NO_DATE = null;
        private const string NO_COMPOSER = null;
        private const string NO_PERFORMER = null;
        private const string NO_COMMENT = null;
        private const int NO_DISC = -1;
        private const int NO_POS = -1;
        private const int NO_ID = -1;

        private readonly string file;
        private readonly int time;
        private readonly string album;
        private readonly string artist;
        private readonly string title;
        private readonly string track;
        private readonly string name;
        private readonly string genre;
        private readonly string date;
        private readonly string composer;
        private readonly string performer;
        private readonly string comment;
        private readonly int disc;
        private readonly int pos;
        private readonly int id;
        /// <summary>
        /// The name and path of the file.
        /// </summary>
        public string File { get { return this.file; } }
        /// <summary>
        /// The length of the file in seconds.
        /// </summary>
        public int Time { get { return this.time; } }
        /// <summary>
        /// The album of the file.
        /// </summary>
        public string Album { get { return this.album; } }
        /// <summary>
        /// The artist of the file.
        /// </summary>
        public string Artist { get { return this.artist; } }
        /// <summary>
        /// The title of the file.
        /// </summary>
        public string Title { get { return this.title; } }
        /// <summary>
        /// The value of the track property of the file.
        /// </summary>
        public string Track { get { return this.track; } }
        /// <summary>
        /// The name of the song.
        /// </summary>
        public string Name { get { return this.name; } }
        /// <summary>
        /// The genre of the song.
        /// </summary>
        public string Genre { get { return this.genre; } }
        /// <summary>
        /// The date the song was released.
        /// </summary>
        public string Date { get { return this.date; } }
        /// <summary>
        /// The composer of the song.
        /// </summary>
        public string Composer { get { return this.composer; } }
        /// <summary>
        /// The performer of the song.
        /// </summary>
        public string Performer { get { return this.performer; } }
        /// <summary>
        /// A comment to the file.
        /// </summary>
        public string Comment { get { return this.comment; } }
        /// <summary>
        /// The number of the disc on a multidisc album.
        /// </summary>
        public int Disc { get { return this.disc; } }
        /// <summary>
        /// The index of the file in a playlist.
        /// </summary>
        public int Pos { get { return this.pos; } }
        /// <summary>
        /// The id of the file in a playlist.
        /// </summary>
        public int Id { get { return this.id; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Time"/> property set.
        /// </summary>
        public bool HasTime { get { return this.time != NO_TIME; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Album"/> property set.
        /// </summary>
        public bool HasAlbum { get { return this.album != NO_ALBUM; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Artist"/> property set.
        /// </summary>
        public bool HasArtist { get { return this.artist != NO_ARTIST; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Title"/> property set.
        /// </summary>
        public bool HasTitle { get { return this.title != NO_TITLE; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Track"/> property set.
        /// </summary>
        public bool HasTrack { get { return this.track != NO_TRACK; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Name"/> property set.
        /// </summary>
        public bool HasName { get { return this.name != NO_NAME; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Genre"/> property set.
        /// </summary>
        public bool HasGenre { get { return this.genre != NO_GENRE; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Date"/> property set.
        /// </summary>
        public bool HasDate { get { return this.date != NO_DATE; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Composer"/> property set.
        /// </summary>
        public bool HasComposer { get { return this.composer != NO_COMPOSER; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Performer"/> property set.
        /// </summary>
        public bool HasPerformer { get { return this.performer != NO_PERFORMER; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Comment"/> property set.
        /// </summary>
        public bool HasComment { get { return this.comment != NO_COMMENT; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Disc"/> property set.
        /// </summary>
        public bool HasDisc { get { return this.disc != NO_DISC; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Pos"/> property set.
        /// </summary>
        public bool HasPos { get { return this.pos != NO_POS; } }
        /// <summary>
        /// If the MpdFile has the <see cref="Id"/> property set.
        /// </summary>
        public bool HasId { get { return this.id != NO_ID; } }
        /// <summary>
        /// Creates a new MpdFile.
        /// </summary>
        /// <param name="file">The name and path of the file.</param>
        /// <param name="time">The length of the file in seconds.</param>
        /// <param name="album">The album of the file.</param>
        /// <param name="artist">The artist of the file.</param>
        /// <param name="title">The title of the file.</param>
        /// <param name="track">The value of the track property of the file.</param>
        /// <param name="name">The name of the song.</param>
        /// <param name="genre">The genre of the song.</param>
        /// <param name="date">The date the song was released.</param>
        /// <param name="composer">The composer of the song.</param>
        /// <param name="performer">The performer of the song.</param>
        /// <param name="comment">A comment to the file.</param>
        /// <param name="disc">The number of the disc on a multidisc album.</param>
        /// <param name="pos">The index of the file in a playlist.</param>
        /// <param name="id">The id of the file in a playlist.</param>
        public MpdFile(string file, 
            int time, 
            string album, 
            string artist, 
            string title, 
            string track, 
            string name, 
            string genre, 
            string date,
            string composer,
            string performer,
            string comment,
            int disc,
            int pos,
            int id)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            this.file = file;
            this.time = time;
            this.album = album;
            this.artist = artist;
            this.title = title;
            this.track = track;
            this.name = name;
            this.genre = genre;
            this.date = date;
            this.composer = composer;
            this.performer = performer;
            this.comment = comment;
            this.disc = disc;
            this.pos = pos;
            this.id = id;
        }
        /// <summary>
        /// A string containing all the properties of the file.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            appendString(builder, TAG_FILE, this.file);
            if (this.HasTime)
                appendInt(builder, TAG_TIME, this.time);
            if (this.HasAlbum)
                appendString(builder, TAG_ALBUM, this.album);
            if (this.HasArtist)
                appendString(builder, TAG_ARTIST, this.artist);
            if (this.HasTitle)
                appendString(builder, TAG_TITLE, this.title);
            if (this.HasTrack)
                appendString(builder, TAG_TRACK, this.track);
            if (this.HasName)
                appendString(builder, TAG_NAME, this.name);
            if (this.HasGenre)
                appendString(builder, TAG_GENRE, this.genre);
            if (this.HasDate)
                appendString(builder, TAG_DATE, this.date);
            if (this.HasComposer)
                appendString(builder, TAG_COMPOSER, this.composer);
            if (this.HasPerformer)
                appendString(builder, TAG_PERFORMER, this.performer);
            if (this.HasComment)
                appendString(builder, TAG_COMMENT, this.comment);
            if (this.HasDisc)
                appendInt(builder, TAG_DISC, this.disc);
            if (this.HasPos)
                appendInt(builder, TAG_POS, this.pos);
            if (this.HasId)
                appendInt(builder, TAG_ID, this.id);

            return builder.ToString();
        }

        private static void appendString(StringBuilder builder, string name, string value)
        {
            builder.Append(name);
            builder.Append(": ");
            builder.Append(value);
            builder.AppendLine();
        }

        private static void appendInt(StringBuilder builder, string name, int value)
        {
            builder.Append(name);
            builder.Append(": ");
            builder.Append(value);
            builder.AppendLine();
        }
        /// <summary>
        /// Returns a MpdFile object from a MpdResponse object.
        /// </summary>
        /// <param name="response">The response of the MPD server.</param>
        /// <returns>A new MpdFile object build from the MpdResponse object.</returns>
        public static MpdFile build(MpdResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            string file = null;
            int time = NO_TIME;
            string album = NO_ALBUM;
            string artist = NO_ARTIST;
            string title = NO_TITLE;
            string track = NO_TRACK;
            string name = NO_NAME;
            string genre = NO_GENRE;
            string date = NO_DATE;
            string composer = NO_COMPOSER;
            string performer = NO_PERFORMER;
            string comment = NO_COMMENT;
            int disc = NO_DISC;
            int pos = NO_POS;
            int id = NO_ID;


            foreach (KeyValuePair<string, string> line in response)
            {
                if( line.Key != null )
                    switch (line.Key)
                    {
                        case TAG_FILE:
                            file = line.Value;
                            break;
                        case TAG_TIME:
                            int tryTime;
                            if( int.TryParse( line.Value, out tryTime ) )
                                time = tryTime;
                            break;
                        case TAG_ALBUM:
                            album = line.Value;
                            break;
                        case TAG_ARTIST:
                            artist = line.Value;
                            break;
                        case TAG_TITLE:
                            title = line.Value;
                            break;
                        case TAG_TRACK:
                            track = line.Value;
                            break;
                        case TAG_NAME:
                            name = line.Value;
                            break;
                        case TAG_GENRE:
                            genre = line.Value;
                            break;
                        case TAG_DATE:
                            date = line.Value;
                            break;
                        case TAG_COMPOSER:
                            composer = line.Value;
                            break;
                        case TAG_PERFORMER:
                            performer = line.Value;
                            break;
                        case TAG_COMMENT:
                            comment = line.Value;
                            break;
                        case TAG_DISC:
                            int tryDisc;
                            if (int.TryParse(line.Value, out tryDisc))
                                disc = tryDisc;
                            break;
                        case TAG_POS:
                            int tryPos;
                            if (int.TryParse(line.Value, out tryPos))
                                pos = tryPos;
                            break;
                        case TAG_ID:
                            int tryId;
                            if (int.TryParse(line.Value, out tryId))
                                id = tryId;
                            break;
                    }
            }

            if (file == null)
                return null;
            else
                return new MpdFile(
                    file,
                    time,
                    album,
                    artist,
                    title,
                    track,
                    name,
                    genre,
                    date,
                    composer,
                    performer,
                    comment,
                    disc,
                    pos,
                    id);
        }
        /// <summary>
        /// Builds a list of MpdFile objects from a MpdResponse object.
        /// </summary>
        /// <param name="response">The MpdResponse object to build the list of MpdFiles from.</param>
        /// <returns>A list ob MpdFiles built from the MpdResponse object.</returns>
        public static List<MpdFile> buildList(MpdResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            List<MpdFile> ret = new List<MpdFile>();

            string file = null;
            int time = NO_TIME;
            string album = NO_ALBUM;
            string artist = NO_ARTIST;
            string title = NO_TITLE;
            string track = NO_TRACK;
            string name = NO_NAME;
            string genre = NO_GENRE;
            string date = NO_DATE;
            string composer = NO_COMPOSER;
            string performer = NO_PERFORMER;
            string comment = NO_COMMENT;
            int disc = NO_DISC;
            int pos = NO_POS;
            int id = NO_ID;


            foreach (KeyValuePair<string, string> line in response)
            {
                if( line.Key != null )
                    switch (line.Key)
                    {
                        case TAG_FILE:
                            if( file != null )
                                ret.Add( new MpdFile( 
                                    file, 
                                    time, 
                                    album, 
                                    artist, 
                                    title, 
                                    track, 
                                    name, 
                                    genre, 
                                    date, 
                                    composer, 
                                    performer, 
                                    comment,
                                    disc,
                                    pos,
                                    id ) );

                            file = line.Value;

                            time = NO_TIME;
                            album = NO_ALBUM;
                            artist = NO_ARTIST;
                            title = NO_TITLE;
                            track = NO_TRACK;
                            name = NO_NAME;
                            genre = NO_GENRE;
                            date = NO_DATE;
                            composer = NO_COMPOSER;
                            performer = NO_PERFORMER;
                            comment = NO_COMMENT;
                            disc = NO_DISC;
                            pos = NO_POS;
                            id = NO_ID;

                            break;
                        case TAG_TIME:
                            int tryTime;
                            if( int.TryParse( line.Value, out tryTime ) )
                                time = tryTime;
                            break;
                        case TAG_ALBUM:
                            album = line.Value;
                            break;
                        case TAG_ARTIST:
                            artist = line.Value;
                            break;
                        case TAG_TITLE:
                            title = line.Value;
                            break;
                        case TAG_TRACK:
                            track = line.Value;
                            /*
                            int tryTrack;
                            if (int.TryParse(line.Value, out tryTrack))
                                track = tryTrack;
                             */
                            break;
                        case TAG_NAME:
                            name = line.Value;
                            break;
                        case TAG_GENRE:
                            genre = line.Value;
                            break;
                        case TAG_DATE:
                            date = line.Value;
                            break;
                        case TAG_COMPOSER:
                            composer = line.Value;
                            break;
                        case TAG_PERFORMER:
                            performer = line.Value;
                            break;
                        case TAG_COMMENT:
                            comment = line.Value;
                            break;
                        case TAG_DISC:
                            int tryDisc;
                            if (int.TryParse(line.Value, out tryDisc))
                                disc = tryDisc;
                            break;
                        case TAG_POS:
                            int tryPos;
                            if (int.TryParse(line.Value, out tryPos))
                                pos = tryPos;
                            break;
                        case TAG_ID:
                            int tryId;
                            if (int.TryParse(line.Value, out tryId))
                                id = tryId;
                            break;
                    }
            }

            if (file != null)
                ret.Add(new MpdFile(
                    file,
                    time,
                    album,
                    artist,
                    title,
                    track,
                    name,
                    genre,
                    date,
                    composer,
                    performer,
                    comment,
                    disc,
                    pos,
                    id ));

            return ret;
        }
    }
}
