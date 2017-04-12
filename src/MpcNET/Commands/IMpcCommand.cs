using System.Collections.Generic;

namespace MpcNET.Commands
{
    public interface IMpcCommand<out T>
    {
        string Value { get; }

        T FormatResponse(IList<KeyValuePair<string, string>> response);
    }
}