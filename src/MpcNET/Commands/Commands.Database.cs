using System.Collections.Generic;
using MpcNET.Commands.Database;
using MpcNET.Tags;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public static partial class Command
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/database.html
        /// </summary>
        public static class Database
        {
            public static IMpcCommand<IEnumerable<IMpdFile>> Find(ITag tag, string searchText)
            {
                return new FindCommand(tag, searchText);
            }

            public static IMpcCommand<string> Update(string uri = null)
            {
                return new UpdateCommand(uri);
            }

            // TODO: count
            // TODO: rescan
            public static IMpcCommand<IEnumerable<MpdDirectory>> ListAll()
            {
                return new ListAllCommand();
            }
        }
    }
}