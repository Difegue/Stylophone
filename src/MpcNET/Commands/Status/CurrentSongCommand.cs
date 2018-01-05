using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Status
{
    internal class CurrentSongCommand : IMpcCommand<IMpdFile>
    {
        public string Value => "currentsong";

        public IMpdFile FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return MpdFile.Create(response, 0).mpdFile;
        }
    }
}