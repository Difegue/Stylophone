using System;

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

            public class Find : IMpcCommand
            {
                private readonly ITag _tag;
                private readonly string _searchText;

                public Find(ITag tag, string searchText)
                {
                    _tag = tag;
                    _searchText = searchText;
                }

                public string Value => string.Join(" ", "find", _tag.Value, _searchText);

                public object ParseResponse(object response)
                {
                    throw new NotImplementedException();
                }
            }

            public class List : IMpcCommand
            {
                private readonly ITag _tag;

                public List(ITag tag)
                {
                    _tag = tag;
                }

                public string Value => string.Join(" ", "list", _tag);

                public object ParseResponse(object response)
                {
                    throw new NotImplementedException();
                }
            }

            // TODO: findadd

            public class ListAll : IMpcCommand
            {
                private readonly string _path;

                public ListAll(string path)
                {
                    _path = path;
                }

                public string Value => string.Join(" ", "listall", _path);

                public object ParseResponse(object response)
                {
                    throw new NotImplementedException();
                }
            }

            // TODO: listallinfo
            // TODO: listfiles
            // TODO: lsinfo
            // TODO: readcomments
            // TODO: search
            // TODO: searchadd
            // TODO: searchaddpl

            public class Update : IMpcCommand
            {
                public string Value => "update";

                public object ParseResponse(object response)
                {
                    throw new NotImplementedException();
                }
            }

            // TODO: rescan
        }
    }
}