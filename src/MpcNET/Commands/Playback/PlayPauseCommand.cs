using System.Collections.Generic;

namespace MpcNET.Commands.Playback
{
    internal class PlayPauseCommand : IMpcCommand<string>
    {
        public string Value => string.Join(" ", "pause");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}