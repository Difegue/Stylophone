using System.Collections.Generic;

namespace MpcNET.Commands.Output
{
    /// <summary>
    /// Turns an output off.
    /// </summary>
    public class DisableOutputCommand : IMpcCommand<string>
    {
        private readonly int outputId;

        public DisableOutputCommand(int outputId)
        {
            this.outputId = outputId;
        }

        public string Value => string.Join(" ", "disableoutput", this.outputId);

        public string FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            // Response should be empty.
            return string.Join(", ", response);
        }
    }
}