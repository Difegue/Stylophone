using System.Collections.Generic;
using MpcNET.Tags;
using MpcNET.Types;

namespace MpcNET.Commands.Database
{
    /// <summary>
    /// Finds songs in the database that is exactly "searchText".
    /// </summary>
    internal class FindCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly ITag _tag;
        private readonly string _searchText;

        public FindCommand(ITag tag, string searchText)
        {
            _tag = tag;
            _searchText = searchText;
        }

        public string Value => string.Join(" ", "find", _tag.Value, _searchText);

        public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }

    // TODO: rescan
}
