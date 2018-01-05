using System.Collections.Generic;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Deletes the song SONGID from the playlist
    /// </summary>
    internal class DeleteIdCommand : IMpcCommand<string>
    {
        private readonly int songId;

        public DeleteIdCommand(int songId)
        {
            this.songId = songId;
        }

        public string Value => string.Join(" ", "deleteid", this.songId);

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}