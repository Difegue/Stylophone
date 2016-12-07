using System.Collections.Generic;

namespace LibMpc
{
    public partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/database.html
        /// </summary>
        public class Database
        {
            // TODO: count
            
            public class Find : IMpcCommand<string>
            {
                private readonly ITag _tag;
                private readonly string _searchText;

                public Find(ITag tag, string searchText)
                {
                    _tag = tag;
                    _searchText = searchText;
                }

                public string Value => string.Join(" ", "find", _tag.Value, _searchText);

                public IDictionary<string, string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    return response.ToDefaultDictionary();
                }
            }

            public class List : IMpcCommand<string>
            {
                private readonly ITag _tag;

                public List(ITag tag)
                {
                    _tag = tag;
                }

                public string Value => string.Join(" ", "list", _tag);

                public IDictionary<string, string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    return response.ToDefaultDictionary();
                }
            }

            // TODO: findadd

            public class ListAll : IMpcCommand<string>
            {
                private readonly string _path;

                public ListAll(string path)
                {
                    _path = path;
                }

                public string Value => string.Join(" ", "listall", _path);

                public IDictionary<string, string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    return response.ToDefaultDictionary();
                }
            }

            // TODO: listallinfo
            // TODO: listfiles
            // TODO: lsinfo
            // TODO: readcomments
            // TODO: search
            // TODO: searchadd
            // TODO: searchaddpl

            public class Update : IMpcCommand<string>
            {
                // TODO: Extend command: < update [URI] >
                public string Value => "update";

                public IDictionary<string, string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    return response.ToDefaultDictionary();
                }
            }
            
            // TODO: rescan
        }
    }
}