using System.Collections.Generic;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Adds the file URI to the playlist (directories add recursively). URI can also be a single file.
    /// </summary>
    internal class AddCommand : IMpcCommand<string>
    {
        private readonly string _uri;

        public AddCommand(string uri)
        {
            _uri = uri;
        }

        public string Value => string.Join(" ", "add", $"\"{_uri}\"");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}
