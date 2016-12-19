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
            // TODO: commands
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
