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

                public IDictionary<string, string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    return response.ToDefaultDictionary();
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

                public IDictionary<string, string> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    return response.ToDefaultDictionary();
                }
            }

            // TODO: toggleoutput // Turns an output on or off, depending on the current state.

            /// <summary>
            /// Shows information about all outputs.
            /// </summary>
            public class Outputs : IMpcCommand<IList<IDictionary<string, string>>>
            {
                public string Value => "outputs";
                
                public IDictionary<string, IList<IDictionary<string, string>>> FormatResponse(IList<KeyValuePair<string, string>> response)
                {
                    var result = new Dictionary<string, IList<IDictionary<string, string>>>
                    {
                        { "outputs", new List<IDictionary<string, string>>() }
                    };

                    for (var i = 0; i < response.Count; i += 3)
                    {
                        var outputId = response[i].Value;
                        var outputName = response[i + 1].Value;
                        var outputEnabled = response[i + 2].Value;

                        result["outputs"].Add(new Dictionary<string, string>
                        {
                            {"id", outputId},
                            {"name", outputName},
                            {"enabled", outputEnabled}
                        });
                    }

                    return result;
                }
            }
        }
    }
}