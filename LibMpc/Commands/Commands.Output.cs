using System.Collections.Generic;
using LibMpc.Types;

namespace LibMpc
{
    public partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/output_commands.html
        /// </summary>
        public class Output
        {
            /// <summary>
            /// Turns an output off.
            /// </summary>
            public class DisableOutput : IMpcCommand<string>
            {
                private readonly int _outputId;

                public DisableOutput(int outputId)
                {
                    _outputId = outputId;
                }

                public string Value => string.Join(" ", "disableoutput", _outputId);

                public string FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    // TODO:
                    return response.ToString();
                }
            }

            /// <summary>
            /// Turns an output on.
            /// </summary>
            public class EnableOutput : IMpcCommand<string>
            {
                private readonly int _outputId;

                public EnableOutput(int outputId)
                {
                    _outputId = outputId;
                }

                public string Value => string.Join(" ", "enableoutput", _outputId);

                public string FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    // TODO:
                    return response.ToString();
                }
            }

            // TODO: toggleoutput // Turns an output on or off, depending on the current state.

            /// <summary>
            /// Shows information about all outputs.
            /// </summary>
            public class Outputs : IMpcCommand<IEnumerable<MpdOutput>>
            {
                public string Value => "outputs";
                
                public IEnumerable<MpdOutput> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = new List<MpdOutput>();

                    for (var i = 0; i < response.Count; i += 3)
                    {
                        var outputId = int.Parse(response[i].Value);
                        var outputName = response[i + 1].Value;
                        var outputEnabled = bool.Parse(response[i + 2].Value);

                        result.Add(new MpdOutput(outputId, outputName, outputEnabled));
                    }

                    return result;
                }
            }
        }
    }
}