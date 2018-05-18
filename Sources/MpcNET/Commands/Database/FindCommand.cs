// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;
    using MpcNET.Tags;
    using MpcNET.Types;

    /// <summary>
    /// Finds songs in the database that is exactly "searchText".
    /// </summary>
    internal class FindCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly ITag tag;
        private readonly string searchText;

        public FindCommand(ITag tag, string searchText)
        {
            this.tag = tag;
            this.searchText = searchText;
        }

        public string Serialize() => string.Join(" ", "find", this.tag.Value, this.searchText);

        public IEnumerable<IMpdFile> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }

    // TODO: rescan
}
