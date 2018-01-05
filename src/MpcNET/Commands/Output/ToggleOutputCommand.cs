using System.Collections.Generic;

namespace MpcNET.Commands.Output
{
    /// <summary>
    /// Turns an output on or off, depending on the current state.
    /// </summary>
    internal class ToggleOutputCommand : IMpcCommand<string>
    {
        private readonly int outputId;

        public ToggleOutputCommand(int outputId)
        {
            this.outputId = outputId;
        }

        public string Value => string.Join(" ", "toggleoutput", outputId);

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            return string.Join(", ", response);
        }
    }
}