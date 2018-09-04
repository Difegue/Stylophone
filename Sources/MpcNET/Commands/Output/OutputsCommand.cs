// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputsCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Output
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Shows information about all outputs.
    /// https://www.musicpd.org/doc/protocol/output_commands.html.
    /// </summary>
    public class OutputsCommand : IMpcCommand<IEnumerable<MpdOutput>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "outputs";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<MpdOutput> Deserialize(IReadOnlyList<KeyValuePair<string, string>> response)
        {
            var result = new List<MpdOutput>();

            for (var i = 0; i < response.Count; i += 3)
            {
                var outputId = int.Parse(response[i].Value);
                var outputName = response[i + 1].Value;
                var outputEnabled = response[i + 2].Value == "1";

                result.Add(new MpdOutput(outputId, outputName, outputEnabled));
            }

            return result;
        }
    }
}