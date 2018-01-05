using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Playback
{
    internal class PlayCommand : IMpcCommand<string>
    {
        private readonly IMpdFile mpdFile;

        public PlayCommand(IMpdFile mpdFile)
        {
            this.mpdFile = mpdFile;
        }

        public string Value
        {
            get
            {
                if (this.mpdFile.HasId)
                {
                    return string.Join(" ", "playid", this.mpdFile.Id);
                }

                return string.Join(" ", "play", this.mpdFile.Pos);
            }
        }

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}