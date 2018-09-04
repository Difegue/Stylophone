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
    /// https://www.musicpd.org/doc/protocol/database.html.
    /// </summary>
    public class FindCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly ITag tag;
        private readonly string searchText;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>
        public FindCommand(ITag tag, string searchText)
        {
            this.tag = tag;
            this.searchText = searchText;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => string.Join(" ", "find", this.tag.Value, this.searchText);

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFile> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            return MpdFile.CreateList(response);
        }
    }

    // TODO: rescan
}
