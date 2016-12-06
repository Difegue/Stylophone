using System.Collections.Generic;

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

            public class TagTypes : IMpcCommand<string>
            {
                public string Value => "tagtypes";

                public IReadOnlyDictionary<string, IList<string>> FormatResponse(IReadOnlyDictionary<string, IList<string>> response)
                {
                    return response;
                }
            }

            // TODO: urlhandlers
            // TODO: decoders
        }
    }
}
