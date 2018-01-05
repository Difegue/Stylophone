using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Reflection
{
    /// <summary>
    /// Print a list of decoder plugins, followed by their supported suffixes and MIME types.
    /// </summary>
    public class DecodersCommand : IMpcCommand<IEnumerable<MpdDecoderPlugin>>
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