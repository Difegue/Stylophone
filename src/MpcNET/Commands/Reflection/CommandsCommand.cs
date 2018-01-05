using System.Collections.Generic;
using System.Linq;

namespace MpcNET.Commands.Reflection
{
    // config : This command is only permitted to "local" clients (connected via UNIX domain socket).

    /// <summary>
    /// Shows which commands the current user has access to.
    /// </summary>
    public class CommandsCommand : IMpcCommand<IEnumerable<string>>
    {
        public string Value => "commands";

        public IEnumerable<string> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            var result = response.Where(item => item.Key.Equals("command")).Select(item => item.Value);

            return result;
        }
    }
}
