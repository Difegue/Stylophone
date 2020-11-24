// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandList.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Reflection
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// To facilitate faster adding of files etc. you can pass a list of commands all at once using a command list. 
    /// The command list begins with command_list_begin or command_list_ok_begin and ends with command_list_end.
    /// https://www.musicpd.org/doc/html/protocol.html#command-lists.
    /// </summary>
    public class CommandList : IMpcCommand<string>
    {

        private readonly List<IMpcCommand<object>> commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandList"/> class.
        /// </summary>
        /// <param name="mpcCommands">IMpcCommand items to add to the list.</param>
        public CommandList(IEnumerable<IMpcCommand<object>> mpcCommands = null)
        {
            commands = new List<IMpcCommand<object>>();

            if (mpcCommands != null)
                AddRange(mpcCommands);
        }

        /// <summary>
        /// Add a command to the list.
        /// </summary>
        /// <param name="c"></param>
        public void Add(IMpcCommand<object> c) => commands.Add(c);

        /// <summary>
        /// Add a range of commands to the list.
        /// </summary>
        /// <param name="r"></param>
        public void AddRange(IEnumerable<IMpcCommand<object>> r) => commands.AddRange(r);

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize()
        {
            var serializedCommands = commands.Select(c => c.Serialize()).ToList();

            serializedCommands.Insert(0, "command_list_begin");
            serializedCommands.Add("command_list_end");

            return string.Join("\n", serializedCommands.ToArray());
        }

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public string Deserialize(SerializedResponse response)
        {
            return string.Join(", ", response.ResponseValues);
        }
    }
}
