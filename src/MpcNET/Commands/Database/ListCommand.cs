using System.Collections.Generic;
using MpcNET.Tags;

namespace MpcNET.Commands.Database
{
    internal class ListCommand : IMpcCommand<string>
    {
        private readonly ITag _tag;

        public ListCommand(ITag tag)
        {
            _tag = tag;
        }

        public string Value => string.Join(" ", "list", _tag);

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            // TODO:
            return response.ToString();
        }
    }

    // TODO: rescan
}