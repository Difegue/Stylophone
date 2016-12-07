namespace LibMpc
{
    /// <summary>
    /// https://www.musicpd.org/doc/protocol/tags.html
    /// </summary>
    public class MpdTags
    {
        public static ITag Artist { get; } = new Tag("artist");
        public static ITag ArtistSort { get; } = new Tag("artistsort");
        public static ITag Album { get; } = new Tag("album");
        public static ITag AlbumSort { get; } = new Tag("albumsort");
        public static ITag AlbumArtist { get; } = new Tag("albumartist");
        public static ITag AlbumArtistSort { get; } = new Tag("albumartistsort");
        public static ITag Title { get; } = new Tag("title");
        public static ITag Track { get; } = new Tag("track");
        public static ITag Name { get; } = new Tag("name");
        public static ITag Genre { get; } = new Tag("genre");
        public static ITag Date { get; } = new Tag("date");
        public static ITag Composer { get; } = new Tag("composer");
        public static ITag Performer { get; } = new Tag("performer");
        public static ITag Comment { get; } = new Tag("comment");
        public static ITag Disc { get; } = new Tag("disc");
    }
}