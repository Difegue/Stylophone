using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET
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
                    // Response should be empty.
                    return string.Join(", ", response);
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
                    // Response should be empty.
                    return string.Join(", ", response);
                }
            }

            /// <summary>
            /// Turns an output on or off, depending on the current state.
            /// </summary>
            public class ToggleOutput : IMpcCommand<string>
            {
                private readonly int _outputId;

                public ToggleOutput(int outputId)
                {
                    _outputId = outputId;
                }

                public string Value => string.Join(" ", "toggleoutput", _outputId);

                public string FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    // Response should be empty.
                    return string.Join(", ", response);
                }
            }

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
                        var outputEnabled = response[i + 2].Value == "1";

                        result.Add(new MpdOutput(outputId, outputName, outputEnabled));
                    }

                    return result;
                }
            }
        }
    }
}