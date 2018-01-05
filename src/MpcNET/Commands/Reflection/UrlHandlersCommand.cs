using System.Collections.Generic;
using System.Linq;

namespace MpcNET.Commands.Reflection
{
    /// <summary>
    /// Gets a list of available URL handlers.
    /// </summary>
    public class UrlHandlersCommand : IMpcCommand<IEnumerable<string>>
    {
        public string Value => "urlhandlers";

        public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            var result = response.Where(item => item.Key.Equals("handler")).Select(item => item.Value);

            return result;
        }
    }
}