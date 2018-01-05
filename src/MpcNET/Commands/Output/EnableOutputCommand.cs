using System.Collections.Generic;

namespace MpcNET.Commands.Output
{
    /// <summary>
    /// Turns an output on.
    /// </summary>
    internal class EnableOutputCommand : IMpcCommand<string>
    {
        private readonly int _outputId;

        public EnableOutputCommand(int outputId)
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
}