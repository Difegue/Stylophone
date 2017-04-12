namespace MpcNET.Tags
{
    /// <summary>
    /// https://www.musicpd.org/doc/protocol/database.html : find {TYPE} {WHAT} [...] [window START:END]
    /// </summary>
    public class FindTags
    {
        public static ITag Any { get; } = new Tag("any");
        public static ITag File { get; } = new Tag("file");
        public static ITag Base { get; } = new Tag("base");
        public static ITag ModifiedSince { get; } = new Tag("modified-since");
    }
}