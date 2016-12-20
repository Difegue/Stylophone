using System.Collections.Generic;
using System.Linq;
using LibMpc.Types;

namespace LibMpc
{
    public partial class Commands
    {
        public static class Playlists
        {
            /// <summary>
            /// https://www.musicpd.org/doc/protocol/queue.html
            /// </summary>
            public static class Current
            {
                
            }

            /// <summary>
            /// https://www.musicpd.org/doc/protocol/playlist_files.html
            /// </summary>
            public static class Stored
            {
                public class ListPlaylist : IMpcCommand<IEnumerable<IMpdFilePath>>
                {
                    private readonly string _playlistName;

                    public ListPlaylist(string playlistName)
                    {
                        _playlistName = playlistName;
                    }

                    public string Value => string.Join(" ", "listplaylist", $"\"{_playlistName}\"");

                    public IEnumerable<IMpdFilePath> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var results = response.Where(line => line.Key.Equals("file")).Select(line => new MpdFile(line.Value));

                        return results;
                    }
                }

                /// <summary>
                /// Prints a list of the playlist directory.
                /// </summary>
                public class ListPlaylists : IMpcCommand<IEnumerable<MpdPlaylist>>
                {
                    public string Value => "listplaylists";

                    public IEnumerable<MpdPlaylist> FormatResponse(IList<KeyValuePair<string, string>> response)
                    {
                        var result = new List<MpdPlaylist>();

                        foreach (var line in response)
                        {
                            if (line.Key.Equals("playlist"))
                            {
                                result.Add(new MpdPlaylist(line.Value));
                            }
                            else if (line.Key.Equals("Last-Modified"))
                            {
                                result.Last().AddLastModified(line.Value);
                            }
                        }

                        return result;
                    }
                }
            }
        }
    }
}