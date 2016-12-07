using System.Collections.Generic;
using System.Linq;

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
            
            public class Find : IMpcCommand<IList<IDictionary<string, string>>>
            {
                private readonly ITag _tag;
                private readonly string _searchText;

                public Find(ITag tag, string searchText)
                {
                    _tag = tag;
                    _searchText = searchText;
                }

                public string Value => string.Join(" ", "find", _tag.Value, _searchText);

                public IDictionary<string, IList<IDictionary<string, string>>> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var results = new Dictionary<string, IList<IDictionary<string, string>>>
                    {
                        { "files", new List<IDictionary<string, string>>() }
                    };

                    foreach (var line in response)
                    {
                        if (line.Key.Equals("file"))
                        {
                            results["files"].Add(new Dictionary<string, string> { { "file", line.Value } });
                        }
                        else
                        {
                            results["files"].Last().Add(line.Key, line.Value);
                        }
                    }

                    return results;
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