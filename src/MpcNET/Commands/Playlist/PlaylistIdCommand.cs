using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Displays song ID in the playlist.
    /// </summary>
    internal class PlaylistIdCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly int _songId;

        public PlaylistIdCommand(int songId)
        {
            _songId = songId;
        }

        public string Value => string.Join((string) " ", new[] {"playlistid"}, _songId);

        public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }
}