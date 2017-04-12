using System.Collections.Generic;
using System.Linq;

namespace LibMpc
{
    public interface IMpcCommand<out T>
    {
        string Value { get; }

        T FormatResponse(IList<KeyValuePair<string, string>> response);
    }
}