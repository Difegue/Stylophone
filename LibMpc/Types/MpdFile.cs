namespace LibMpc.Types
{
    public class MpdFileBuidler
    {
        private const string TagFile = "file";
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

        private MpdFile _mpdFile;

        public bool IsInitialized => _mpdFile != null;

        public MpdFileBuidler Init(string file)
        {
            _mpdFile = new MpdFile(file);
            return this;
        }

        public MpdFileBuidler WithProperty(string tag, string value)
        {
            _mpdFile.CheckNotNull();

            // TODO: Parse tag

            return this;
        }

        public MpdFile Build()
        {
            return _mpdFile;
        }
    }


    /// <summary>
    /// The MpdFile class contains all meta data for a file of the MPD.
    /// </summary>
    public class MpdFile
    {
        public MpdFile(string file)
        {
            File = file;
        }

        public string File { get; }
        public int Time { get; internal set;  } = -1;
        public string Album { get; internal set; } = string.Empty;
        public string Artist { get; internal set; } = string.Empty;
        public string Title { get; internal set; } = string.Empty;
        public string Track { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
        public string Genre { get; internal set; } = string.Empty;
        public string Date { get; internal set; } = string.Empty;
        public string Composer { get; internal set; } = string.Empty;
        public string Performer { get; internal set; } = string.Empty;
        public string Comment { get; internal set; } = string.Empty;
        public int Disc { get; internal set; } = -1;
        public int Pos { get; internal set; } = -1;
        public int Id { get; internal set; } = -1;
    }
}
