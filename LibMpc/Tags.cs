namespace LibMpc
{
    /// <summary>
    /// https://www.musicpd.org/doc/protocol/tags.html
    /// </summary>
    public class Tags
    {
        internal class Tag : ITag
        {
            internal Tag(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public ITag Artist { get; } = new Tag("artist");
        public ITag ArtistSort { get; } = new Tag("artistsort");
        public ITag Album { get; } = new Tag("album");
        public ITag AlbumSort { get; } = new Tag("albumsort");
        public ITag AlbumArtist { get; } = new Tag("albumartist");
        public ITag AlbumArtistSort { get; } = new Tag("albumartistsort");
        public ITag Title { get; } = new Tag("title");
        public ITag Track { get; } = new Tag("track");
        public ITag Name { get; } = new Tag("name");
        public ITag Genre { get; } = new Tag("genre");
        public ITag Date { get; } = new Tag("date");
        public ITag Composer { get; } = new Tag("composer");
        public ITag Performer { get; } = new Tag("performer");
        public ITag Comment { get; } = new Tag("comment");
        public ITag Disc { get; } = new Tag("disc");
    }

    public interface ITag
    {
        string Value { get; }
    }
}