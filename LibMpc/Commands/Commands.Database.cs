using System.Collections.Generic;
using LibMpc.Types;

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
            
            public class Find : IMpcCommand<IEnumerable<MpdFile>>
            {
                private readonly ITag _tag;
                private readonly string _searchText;

                public Find(ITag tag, string searchText)
                {
                    _tag = tag;
                    _searchText = searchText;
                }

                public string Value => string.Join(" ", "find", _tag.Value, _searchText);

                public IEnumerable<MpdFile> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var results = new List<MpdFile>();

                    var fileBuilder = new MpdFileBuidler();
                    foreach (var line in response)
                    {
                        if (line.Key.Equals("file"))
                        {
                            if (fileBuilder.IsInitialized)
                            {
                                results.Add(fileBuilder.Build());
                            }

                            fileBuilder.Init(line.Value);
                        }
                        else
                        {
                            fileBuilder.WithProperty(line.Key, line.Value);
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

            public class ListAll : IMpcCommand<MpdDirectory>
            {
                public string Value => "listall";

                public MpdDirectory FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    // Add by default the root directory
                    var rootDirectory = new MpdDirectory("/"); 

                    foreach (var line in response)
                    {
                        if (line.Key.Equals("file"))
                        {
                            rootDirectory.AddFile(line.Value);
                        }

                        if (line.Key.Equals("directory"))
                        {
                            rootDirectory.AddDirectory(line.Value);
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