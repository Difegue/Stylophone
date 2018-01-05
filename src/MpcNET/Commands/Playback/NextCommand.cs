using System.Collections.Generic;

namespace MpcNET.Commands.Playback
{
    internal class NextCommand : IMpcCommand<string>
    {
        public string Value => string.Join(" ", "next");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}
