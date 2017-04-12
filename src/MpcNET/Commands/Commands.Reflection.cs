using System.Collections.Generic;
using System.Linq;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/reflection_commands.html
        /// </summary>
        public static class Reflection
        {
            // config : This command is only permitted to "local" clients (connected via UNIX domain socket).

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

            // TODO: notcommands : Shows which commands the current user does not have access to.

            /// <summary>
            /// Shows a list of available song metadata.
            /// </summary>
            public class TagTypes : IMpcCommand<IEnumerable<string>>
            {
                public string Value => "tagtypes";

                public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = response.Where(item => item.Key.Equals("tagtype")).Select(item => item.Value);

                    return result;
                }
            }

            /// <summary>
            /// Gets a list of available URL handlers.
            /// </summary>
            public class UrlHandlers : IMpcCommand<IEnumerable<string>>
            {
                public string Value => "urlhandlers";

                public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = response.Where(item => item.Key.Equals("handler")).Select(item => item.Value);

                    return result;
                }
            }

            /// <summary>
            /// Print a list of decoder plugins, followed by their supported suffixes and MIME types.
            /// </summary>
            public class Decoders : IMpcCommand<IEnumerable<MpdDecoderPlugin>>
            {
                public string Value => "decoders";

                public IEnumerable<MpdDecoderPlugin> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = new List<MpdDecoderPlugin>();

                    var mpdDecoderPlugin = MpdDecoderPlugin.Empty;
                    foreach (var line in response)
                    {
                        if (line.Key.Equals("plugin"))
                        {
                            if (mpdDecoderPlugin.IsInitialized)
                            {
                                result.Add(mpdDecoderPlugin);
                            }

                            mpdDecoderPlugin = new MpdDecoderPlugin(line.Value);
                        }

                        if (line.Key.Equals("suffix") && mpdDecoderPlugin.IsInitialized)
                        {
                            mpdDecoderPlugin.AddSuffix(line.Value);
                        }

                        if (line.Key.Equals("mime_type") && mpdDecoderPlugin.IsInitialized)
                        {
                            mpdDecoderPlugin.AddMediaType(line.Value);
                        }
                    }

                    return result;
                }
            }
        }
    }
}
