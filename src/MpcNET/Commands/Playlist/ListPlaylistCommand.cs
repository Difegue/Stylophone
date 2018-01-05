using System.Collections.Generic;
using System.Linq;
using MpcNET.Types;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Lists the songs in the playlist.
    /// </summary>
    internal class ListPlaylistCommand : IMpcCommand<IEnumerable<IMpdFilePath>>
    {
        private readonly string _playlistName;

        public ListPlaylistCommand(string playlistName)
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
}