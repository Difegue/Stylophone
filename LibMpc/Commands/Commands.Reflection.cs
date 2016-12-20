using System.Collections.Generic;
using System.Linq;

namespace LibMpc
{
    public partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/reflection_commands.html
        /// </summary>
        public static class Reflection
        {
            // TODO: config

            /// <summary>
            /// Shows which commands the current user has access to.
            /// </summary>
            public class Commands : IMpcCommand<IEnumerable<string>>
            {
                public string Value => "commands";

                public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = response.Where(item => item.Key.Equals("command")).Select(item => item.Value);

                    return result;
                }
            }

            // TODO: notcommands

            public class TagTypes : IMpcCommand<IEnumerable<string>>
            {
                public string Value => "tagtypes";

                public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = response.Where(item => item.Key.Equals("tagtype")).Select(item => item.Value);

                    return result;
                }
            }

            // TODO: urlhandlers
            // TODO: decoders
        }
    }
}
