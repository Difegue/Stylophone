// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListAllCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Types;

    /// <summary>
    /// Lists all songs and directories in URI.
    /// </summary>
    internal class ListAllCommand : IMpcCommand<IEnumerable<MpdDirectory>>
    {
        public string Serialize() => "listall";

        public IEnumerable<MpdDirectory> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var rootDirectory = new List<MpdDirectory>
            {
                new MpdDirectory("/"), // Add by default the root directory
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