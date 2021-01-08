// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdFile.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Types
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The MpdFile class contains all meta data for a file of the MPD.
    /// </summary>
    internal class MpdFile : IMpdFile
    {
        internal const string TagFile = "file";
        internal const string TagTime = "Time";
        internal const string TagArtist = "Artist";
        internal const string TagAlbum = "Album";
        internal const string TagTitle = "Title";
        internal const string TagTrack = "Track";
        internal const string TagName = "Name";
        internal const string TagGenre = "Genre";
        internal const string TagDate = "Date";
        internal const string TagComposer = "Composer";
        internal const string TagPerformer = "Performer";
        internal const string TagComment = "Comment";
        internal const string TagDisc = "Disc";
        internal const string TagPos = "Pos";
        internal const string TagId = "Id";

        internal const int NoTime = -1;
        internal const string NoAlbum = null;
        internal const string NoArtist = null;
        internal const string NoTitle = null;
        internal const string NoTrack = null;
        internal const string NoName = null;
        internal const string NoGenre = null;
        internal const string NoDate = null;
        internal const string NoComposer = null;
        internal const string NoPerformer = null;
        internal const string NoComment = null;
        internal const int NoDisc = -1;
        internal const int NoPos = -1;
        internal const int NoId = -1;

        internal MpdFile(
            string path,
            int time = NoTime,
            string album = NoAlbum,
            string artist = NoArtist,
            string title = NoTitle,
            string track = NoTrack,
            string name = NoName,
            string genre = NoGenre,
            string date = NoDate,
            string composer = NoComposer,
            string performer = NoPerformer,
            string comment = NoComment,
            int disc = NoDisc,
            int pos = NoPos,
            int id = NoId,
            IReadOnlyDictionary<string, string> unknownMetadata = null)
        {
            this.Path = path;
            this.Time = time;
            this.Album = album;
            this.Artist = artist;
            this.Title = title;
            this.Track = track;
            this.Name = name;
            this.Genre = genre;
            this.Date = date;
            this.Composer = composer;
            this.Performer = performer;
            this.Comment = comment;
            this.Disc = disc;
            this.Position = pos;
            this.Id = id;
            this.UnknownMetadata = unknownMetadata ?? new Dictionary<string, string>();
        }

        public string Path { get; }

        public int Time { get; }

        public string Album { get; }

        public string Artist { get; }

        public string Title { get; }

        public string Track { get; }

        public string Name { get; }

        public string Genre { get; }

        public string Date { get; }

        public string Composer { get; }

        public string Performer { get; }

        public string Comment { get; }

        public int Disc { get; }

        public int Position { get; set; }

        public int Id { get; }

        public IReadOnlyDictionary<string, string> UnknownMetadata { get; }

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Time"/> property set.
        /// </summary>
        public bool HasTime => this.Time != NoTime;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Album"/> property set.
        /// </summary>
        public bool HasAlbum => this.Album != NoAlbum;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Artist"/> property set.
        /// </summary>
        public bool HasArtist => this.Artist != NoArtist;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Title"/> property set.
        /// </summary>
        public bool HasTitle => this.Title != NoTitle;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Track"/> property set.
        /// </summary>
        public bool HasTrack => this.Track != NoTrack;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Name"/> property set.
        /// </summary>
        public bool HasName => this.Name != NoName;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Genre"/> property set.
        /// </summary>
        public bool HasGenre => this.Genre != NoGenre;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Date"/> property set.
        /// </summary>
        public bool HasDate => this.Date != NoDate;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Composer"/> property set.
        /// </summary>
        public bool HasComposer => this.Composer != NoComposer;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Performer"/> property set.
        /// </summary>
        public bool HasPerformer => this.Performer != NoPerformer;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Comment"/> property set.
        /// </summary>
        public bool HasComment => this.Comment != NoComment;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Disc"/> property set.
        /// </summary>
        public bool HasDisc => this.Disc != NoDisc;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Position"/> property set.
        /// </summary>
        public bool HasPos => this.Position != NoPos;

        /// <summary>
        /// Gets a value indicating whether the MpdFile has the <see cref="Id"/> property set.
        /// </summary>
        public bool HasId => this.Id != NoId;

        public override string ToString()
        {
            return Title ?? Name ?? Path.Split('/').Last();
        }

        internal static MpdFile Create(string path, int pos)
        {
            return new MpdFile(path, pos: pos);
        }

        internal static (IMpdFile mpdFile, int index) Create(
            IReadOnlyList<KeyValuePair<string, string>> response,
            int startIndex)
        {
            string file;
            if (response.Count <= startIndex)
            {
                return (null, -1);
            }

            var fileKeyValuePair = response[startIndex];
            if (fileKeyValuePair.Key == "file")
            {
                file = fileKeyValuePair.Value;
            }
            else
            {
                return (null, -1);
            }

            int time = NoTime;
            string album = NoAlbum;
            string artist = NoArtist;
            string title = NoTitle;
            string track = NoTrack;
            string name = NoName;
            string genre = NoGenre;
            string date = NoDate;
            string composer = NoComposer;
            string performer = NoPerformer;
            string comment = NoComment;
            int disc = NoDisc;
            int pos = NoPos;
            int id = NoId;
            var unknownMetadata = new Dictionary<string, string>();
            for (var index = startIndex + 1; index < response.Count; index++)
            {
                var line = response[index];
                if (line.Key != null)
                {
                    switch (line.Key)
                    {
                        case TagFile:
                            return (new MpdFile(
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
                                id), index - 1);
                        case TagTime:
                            if (int.TryParse(line.Value, out int tryTime))
                            {
                                time = tryTime;
                            }

                            break;
                        case TagAlbum:
                            album = line.Value;
                            break;
                        case TagArtist:
                            artist = line.Value;
                            break;
                        case TagTitle:
                            title = line.Value;
                            break;
                        case TagTrack:
                            track = line.Value;
                            break;
                        case TagName:
                            name = line.Value;
                            break;
                        case TagGenre:
                            genre = line.Value;
                            break;
                        case TagDate:
                            date = line.Value;
                            break;
                        case TagComposer:
                            composer = line.Value;
                            break;
                        case TagPerformer:
                            performer = line.Value;
                            break;
                        case TagComment:
                            comment = line.Value;
                            break;
                        case TagDisc:
                            if (int.TryParse(line.Value, out var tryDisc))
                            {
                                disc = tryDisc;
                            }

                            break;
                        case TagPos:
                            if (int.TryParse(line.Value, out var tryPos))
                            {
                                pos = tryPos;
                            }

                            break;
                        case TagId:
                            if (int.TryParse(line.Value, out var tryId))
                            {
                                id = tryId;
                            }

                            break;
                        default:
                            // If a similar key has already been added to unknown metadata, add a ' to this second one so it can still be passed through.
                            // (It certainly won't be used though)
                            var key = line.Key;
                            while (unknownMetadata.ContainsKey(key))
                            {
                                key = key + "'";
                            }
                            unknownMetadata.Add(key, line.Value);

                            break;
                    }
                }
            }

            return (new MpdFile(
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
                id), response.Count - 1);
        }

        internal static IEnumerable<IMpdFile> CreateList(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var mpdFiles = new List<IMpdFile>();
            for (var index = 0; index < response.Count; index++)
            {
                var (mpdFile, lastIndex) = Create(response, index);
                if (mpdFile != null)
                {
                    mpdFiles.Add(mpdFile);
                    index = lastIndex;
                }
                else
                {
                    break;
                }
            }

            return mpdFiles;
        }
    }
}