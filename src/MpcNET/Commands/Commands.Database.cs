using System.Collections.Generic;
using System.Linq;
using MpcNET.Tags;
using MpcNET.Types;

namespace MpcNET
{
    public static partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/database.html
        /// </summary>
        public class Database
        {
            // TODO: count

            /// <summary>
            /// Finds songs in the database that is exactly "searchText".
            /// </summary>
            public class Find : IMpcCommand<IEnumerable<IMpdFile>>
            {
                private readonly ITag _tag;
                private readonly string _searchText;

                public Find(ITag tag, string searchText)
                {
                    _tag = tag;
                    _searchText = searchText;
                }

                public string Value => string.Join(" ", "find", _tag.Value, _searchText);

                public IEnumerable<IMpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var results = new List<MpdFile>();

                    foreach (var line in response)
                    {
                        if (line.Key.Equals("file"))
                        {
                            results.Add(new MpdFile(line.Value));
                        }
                        else
                        {
                            results.Last().AddTag(line.Key, line.Value);
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

                public string FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    // TODO:
                    return response.ToString();
                }
            }

            // TODO: findadd

            /// <summary>
            /// Lists all songs and directories in URI.
            /// </summary>
            public class ListAll : IMpcCommand<IEnumerable<MpdDirectory>>
            {
                public string Value => "listall";

                public IEnumerable<MpdDirectory> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var rootDirectory = new List<MpdDirectory>
                    {
                        new MpdDirectory("/") // Add by default the root directory
                    };

                    foreach (var line in response)
                    {
                        if (line.Key.Equals("file"))
                        {
                            rootDirectory.Last().AddFile(line.Value);
                        }

                        if (line.Key.Equals("directory"))
                        {
                            rootDirectory.Add(new MpdDirectory(line.Value));
                        }
                    }

                    return rootDirectory;
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

                public string FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    // TODO:
                    return response.ToString();
                }
            }
            
            // TODO: rescan
        }
    }
}