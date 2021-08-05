// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FindCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Database
{
    using System.Collections.Generic;
    using System.Linq;
    using MpcNET.Tags;
    using MpcNET.Types;

    /// <summary>
    /// Finds songs in the database that contain "searchText".
    /// Since MPD 0.21, search syntax is now (TAG == 'VALUE').
    /// https://www.musicpd.org/doc/html/protocol.html#filters
    /// </summary>
    public class SearchCommand : BaseFilterCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public override string CommandName => "search";
        /// <summary>
        /// 
        /// </summary>
        public override string Operand => "contains";

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>        
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public SearchCommand(ITag tag, string searchText, int windowStart = -1, int windowEnd = -1) : base(tag, searchText, windowStart, windowEnd) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCommand"/> class.
        /// </summary>
        /// <param name="filters">List of key/value filters</param>
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public SearchCommand(List<KeyValuePair<ITag, string>> filters, int windowStart = -1, int windowEnd = -1) : base(filters, windowStart, windowEnd) { }

    }

    /// <summary>
    /// Finds songs in the database that contain "searchText" and adds them to the queue.
    /// Since MPD 0.21, search syntax is now (TAG == 'VALUE').
    /// https://www.musicpd.org/doc/html/protocol.html#filters
    /// </summary>
    public class SearchAddCommand : BaseFilterCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public override string CommandName => "searchadd";
        /// <summary>
        /// 
        /// </summary>
        public override string Operand => "contains";

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>        
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public SearchAddCommand(ITag tag, string searchText, int windowStart = -1, int windowEnd = -1) : base(tag, searchText, windowStart, windowEnd) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCommand"/> class.
        /// </summary>
        /// <param name="filters">List of key/value filters</param>
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public SearchAddCommand(List<KeyValuePair<ITag, string>> filters, int windowStart = -1, int windowEnd = -1) : base(filters, windowStart, windowEnd) { }

    }

    /// <summary>
    /// Finds songs in the database that is exactly "searchText".
    /// Since MPD 0.21, search syntax is now (TAG == 'VALUE').
    /// https://www.musicpd.org/doc/html/protocol.html#filters
    /// </summary>
    public class FindCommand : BaseFilterCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public override string CommandName => "find";
        /// <summary>
        /// 
        /// </summary>
        public override string Operand => "==";

        /// <summary>
        /// Initializes a new instance of the <see cref="FindCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public FindCommand(ITag tag, string searchText, int windowStart = -1, int windowEnd = -1) : base(tag, searchText, windowStart, windowEnd) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FindCommand"/> class.
        /// </summary>
        /// <param name="filters">List of key/value filters</param>
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public FindCommand(List<KeyValuePair<ITag, string>> filters, int windowStart = -1, int windowEnd = -1) : base(filters, windowStart, windowEnd) { }

    }

   
    /// <summary>
    /// Base class for find/search commands.
    /// </summary>
    public abstract class BaseFilterCommand : IMpcCommand<IEnumerable<IMpdFile>>
    {
        private readonly List<KeyValuePair<ITag, string>> filters;
        private readonly int _start;
        private readonly int _end;

        /// <summary>
        /// Name of the command to use when deserializing
        /// </summary>
        public abstract string CommandName { get; }
        /// <summary>
        /// Operand to use between tags and search text. Can be ==, !=, contains...
        /// </summary>
        public abstract string Operand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFilterCommand"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public BaseFilterCommand(ITag tag, string searchText, int windowStart = -1, int windowEnd = -1)
        {
            this.filters = new List<KeyValuePair<ITag, string>>();
            this.filters.Add(new KeyValuePair<ITag, string>(tag, searchText));

            _start = windowStart;
            _end = windowEnd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFilterCommand"/> class.
        /// </summary>
        /// <param name="filters">List of key/value filters</param>
        /// <param name="windowStart">Start of the portion of the results desired</param>
        /// <param name="windowEnd">End of the portion of the results desired</param>
        public BaseFilterCommand(List<KeyValuePair<ITag, string>> filters, int windowStart = -1, int windowEnd = -1)
        {
            this.filters = filters;

            _start = windowStart;
            _end = windowEnd;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            var cmd =
            CommandName + " \"(" +
            string.Join(" AND ",
                  filters.Select(x => $"({x.Key.Value} {Operand} {escape(x.Value)})")
                  ) +
            ")\"";

            if (_start > -1)
            {
                cmd += $" window {_start}:{_end}";
            }

            return cmd;
        }

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<IMpdFile> Deserialize(SerializedResponse response)
        {
            return MpdFile.CreateList(response.ResponseValues);
        }

        private string escape(string value) => string.Format("\\\"{0}\\\"", value.Replace("\\", "\\\\\\").Replace("'", "\\\\'").Replace("\"", "\\\\\\\""));
    }
    // TODO: rescan
}
