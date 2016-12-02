namespace LibMpc
{
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

        public ITag Any { get; } = new Tag("any");
        public ITag Filename { get; } = new Tag("filename");
        public ITag Artist { get; }= new Tag("artist");
        public ITag Album { get; }= new Tag("album");
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