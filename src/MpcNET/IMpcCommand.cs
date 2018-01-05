using System.Collections.Generic;

namespace MpcNET
{
    public interface IMpcCommand<out T>
    {
        string Value { get; }

        T FormatResponse(IList<KeyValuePair<string, string>> response);
    }
}