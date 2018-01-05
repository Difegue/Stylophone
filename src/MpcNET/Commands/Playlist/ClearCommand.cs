using System.Collections.Generic;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Clears the current playlist.
    /// </summary>
    internal class ClearCommand : IMpcCommand<string>
    {
        public string Value => "clear";

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}