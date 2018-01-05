using System.Collections.Generic;

namespace MpcNET.Commands.Playback
{
    internal class StopCommand : IMpcCommand<string>
    {
        public string Value => string.Join(" ", "stop");

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}