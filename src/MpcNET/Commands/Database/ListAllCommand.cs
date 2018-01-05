using System.Collections.Generic;
using System.Linq;
using MpcNET.Types;

namespace MpcNET.Commands.Database
{

    /// <summary>
    /// Lists all songs and directories in URI.
    /// </summary>
    internal class ListAllCommand : IMpcCommand<IEnumerable<MpdDirectory>>
    {
        public string Value => "listall";

        public IEnumerable<MpdDirectory> FormatResponse(IList<KeyValuePair<string, string>> response)
        {
            var rootDirectory = new List<MpdDirectory>
            {
                new MpdDirectory("/") // Add by default the root directory
            };

            foreach (var line in response)
            {
                if (line.Key.Equals("file"))
                {
                    rootDirectory.Last().AddFile(line.Value);
                }

                if (line.Key.Equals("directory"))
                {
                    rootDirectory.Add(new MpdDirectory(line.Value));
                }
            }

            return rootDirectory;
        }
    }

    // TODO: findadd
    // TODO: rescan
}