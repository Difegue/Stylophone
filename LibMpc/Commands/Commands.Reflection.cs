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

            public class TagTypes : IMpcCommand<IList<string>>
            {
                public string Value => "tagtypes";

                IDictionary<string, IList<string>> IMpcCommand<IList<string>>.FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = new Dictionary<string, IList<string>>
                    {
                        { "tagtypes", response.Where(item => item.Key.Equals("tagtype")).Select(item => item.Value).ToList() }
                    };

                    return result;
                }
            }

            // TODO: urlhandlers
            // TODO: decoders
        }
    }
}
