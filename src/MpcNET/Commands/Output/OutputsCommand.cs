using System.Collections.Generic;
using MpcNET.Types;

namespace MpcNET.Commands.Output
{
    /// <summary>
    /// Shows information about all outputs.
    /// </summary>
    public class OutputsCommand : IMpcCommand<IEnumerable<MpdOutput>>
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