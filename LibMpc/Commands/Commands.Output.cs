using System.Collections.Generic;

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

                public IReadOnlyDictionary<string, IList<string>> FormatResponse(IReadOnlyDictionary<string, IList<string>> response)
                {
                    return response;
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

                public IReadOnlyDictionary<string, IList<string>> FormatResponse(IReadOnlyDictionary<string, IList<string>> response)
                {
                    return response;
                }
            }

            // TODO: toggleoutput // Turns an output on or off, depending on the current state.

            /// <summary>
            /// Shows information about all outputs.
            /// </summary>
            public class Outputs : IMpcCommand<Dictionary<string, string>>
            {
                public string Value => "outputs";
                public IReadOnlyDictionary<string, IList<Dictionary<string, string>>> FormatResponse(IReadOnlyDictionary<string, IList<string>> response)
                {
                    var result = new Dictionary<string, IList<Dictionary<string, string>>>
                    {
                        {"outputs", new List<Dictionary<string, string>>()}
                    };

                    for (var i = 0; i < response["outputid"].Count; i++)
                    {
                        result["outputs"].Add(new Dictionary<string, string>
                        {
                            { "id", response["outputid"][i] },
                            { "name", response["outputname"][i] },
                            { "enabled", response["outputenabled"][i] }
                        });
                    }

                    return result;
                }
            }
        }
    }
}