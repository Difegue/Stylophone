using System.Collections.Generic;

namespace MpcNET.Commands.Playlist
{
    /// <summary>
    /// Deletes a song from the playlist.
    /// </summary>
    internal class DeleteCommand : IMpcCommand<string>
    {
        private readonly int position;

        public DeleteCommand(int position)
        {
            this.position = position;
        }

        public string Value => string.Join(" ", "delete", this.position);

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}