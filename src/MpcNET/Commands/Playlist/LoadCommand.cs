using System.Collections.Generic;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Loads the playlist into the current queue.
    /// </summary>
    internal class LoadCommand : IMpcCommand<string>
    {
        private readonly string _playlistName;

        public LoadCommand(string playlistName)
        {
            _playlistName = playlistName;
        }

        public string Value => string.Join(" ", "load", $"\"{_playlistName}\"");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}