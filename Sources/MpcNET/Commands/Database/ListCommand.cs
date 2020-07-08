// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Tags;

    /// <summary>
    /// Lists the specified tag.
    /// https://www.musicpd.org/doc/protocol/database.html.
    /// </summary>
    public class ListCommand : IMpcCommand<List<string>>
    {
        private readonly ITag tag;
        private readonly ITag filterTag;
        private readonly string filterValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public ListCommand(ITag tag)
        {
            this.tag = tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="filterTag">The filter tag.</param>
        /// <param name="filterValue">Filter value.</param>
        public ListCommand(ITag tag, ITag filterTag, string filterValue)
        {
            this.tag = tag;
            this.filterTag = filterTag;
            this.filterValue = filterValue;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            if (this.filterTag == null)
                return string.Join(" ", "list", this.tag.Value);

            return string.Join(" ", "list", this.tag.Value, this.filterTag.Value, escape(this.filterValue));
        }

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public List<string> Deserialize(SerializedResponse response)
        {
            return response.ResponseValues.Select(x => x.Value).ToList();
        }

        private string escape(string value) => string.Format("\"{0}\"", value.Replace("\\", "\\\\").Replace("\"", "\\\""));
    }

    // TODO: rescan
}
