using System.Collections.Generic;

namespace LibMpc.Types
{
    public interface IMpdFilePath
    {
        string File { get; }
    }

    public interface IMpdFile : IMpdFilePath
    {
        int Time { get; }
        string Album { get; } 
        string Artist { get; }
        string Title { get; }
        string Track { get; }
        string Name { get; }
        string Genre { get; }
        string Date { get; }
        string Composer { get; }
        string Performer { get; }
        string Comment { get; }
        int Disc { get; }
        int Pos { get; }
        int Id { get; }
        IDictionary<string, string> UnknownMetadata { get; }
    }
}