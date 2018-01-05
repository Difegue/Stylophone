using System.Collections.Generic;
using System.Linq;
using MpcNET.Types;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Prints a list of the playlist directory.
    /// </summary>
    internal class ListPlaylistsCommand : IMpcCommand<IEnumerable<MpdPlaylist>>
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