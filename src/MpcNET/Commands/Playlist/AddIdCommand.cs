using System.Collections.Generic;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Adds a song to the playlist (non-recursive) and returns the song id.
    /// </summary>
    internal class AddIdCommand : IMpcCommand<string>
    {
        private readonly string _uri;

        public AddIdCommand(string uri)
        {
            _uri = uri;
        }

        public string Value => string.Join(" ", "addid", $"\"{_uri}\"");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}