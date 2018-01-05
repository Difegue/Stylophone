using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Lists the songs with metadata in the playlist.
    /// </summary>
    internal class ListPlaylistInfoCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly string _playlistName;

        public ListPlaylistInfoCommand(string playlistName)
        {
            _playlistName = playlistName;
        }

        public string Value => string.Join(" ", "listplaylistinfo", $"\"{_playlistName}\"");

        public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }
}