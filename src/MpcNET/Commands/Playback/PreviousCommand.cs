using System.Collections.Generic;

namespace MpcNET.Commands.Playback
{
    internal class PreviousCommand : IMpcCommand<string>
    {
        public string Value => string.Join(" ", "previous");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}