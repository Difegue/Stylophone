using System.Collections.Generic;
using System.Linq;
using MpcNET.Types;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Displays the current playlist.
    /// </summary>
    internal class PlaylistCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        public string Value => "playlist";

        public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            var results = response.Select(line => MpdFile.Create(line.Value, int.Parse(line.Key)));

            return results;
        }
    }
}