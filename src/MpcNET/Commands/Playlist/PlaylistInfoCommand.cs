using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Displays a list of all songs in the playlist,
    /// </summary>
    internal class PlaylistInfoCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        public string Value => "playlistinfo";

        public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }
}