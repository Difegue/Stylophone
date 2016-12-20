using System.Collections.Generic;

namespace LibMpc.Types
{
    /// <summary>
    /// The MpdFile class contains all meta data for a file of the MPD.
    /// </summary>
    internal class MpdFile : IMpdFile
    {
        private const string TagTime = "Time";
        private const string TagArtist = "Artist";
        private const string TagAlbum = "Album";
        private const string TagTitle = "Title";
        private const string TagTrack = "Track";
        private const string TagName = "Name";
        private const string TagGenre = "Genre";
        private const string TagDate = "Date";
        private const string TagComposer = "Composer";
        private const string TagPerformer = "Performer";
        private const string TagComment = "Comment";
        private const string TagDisc = "Disc";
        private const string TagPos = "Pos";
        private const string TagId = "Id";

        private readonly IDictionary<string, string> _unknownTags = new Dictionary<string, string>();

        internal MpdFile(string file)
        {
            file.CheckNotNull();

            File = file;
        }

        public string File { get; }
        public int Time { get; private set;  } = -1;
        public string Album { get; private set; } = string.Empty;
        public string Artist { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string Track { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Genre { get; private set; } = string.Empty;
        public string Date { get; private set; } = string.Empty;
        public string Composer { get; private set; } = string.Empty;
        public string Performer { get; private set; } = string.Empty;
        public string Comment { get; private set; } = string.Empty;
        public int Disc { get; private set; } = -1;
        public int Pos { get; private set; } = -1;
        public int Id { get; private set; } = -1;
        public IDictionary<string, string> UnknownTags => _unknownTags;

        internal void AddTag(string tag, string value)
        {
            switch (tag)
            {
                case TagTime:
                    Time = int.Parse(value);
                    break;
                case TagArtist:
                    Artist = value;
                    break;
                case TagAlbum:
                    Album = value;
                    break;
                case TagTitle:
                    Title = value;
                    break;
                case TagTrack:
                    Track = value;
                    break;
                case TagName:
                    Name = value;
                    break;
                case TagGenre:
                    Genre = value;
                    break;
                case TagDate:
                    Date = value;
                    break;
                case TagComposer:
                    Composer = value;
                    break;
                case TagPerformer:
                    Performer = value;
                    break;
                case TagComment:
                    Comment = value;
                    break;
                case TagDisc:
                    Disc = int.Parse(value);
                    break;
                case TagPos:
                    Pos = int.Parse(value);
                    break;
                case TagId:
                    Id = int.Parse(value);
                    break;
                default:
                    _unknownTags.Add(tag, value);
                    break;
            }
        }
    }
}
