using System.Collections.Generic;
using System.Linq;

namespace MpcNET.Commands.Reflection
{
    // TODO: notcommands : Shows which commands the current user does not have access to.

    /// <summary>
    /// Shows a list of available song metadata.
    /// </summary>
    public class TagTypesCommand : IMpcCommand<IEnumerable<string>>
    {
        public string Value => "tagtypes";

        public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            var result = response.Where(item => item.Key.Equals("tagtype")).Select(item => item.Value);

            return result;
        }
    }
}