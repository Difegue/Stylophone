// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputsCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Output
{
    using System.Collections.Generic;
    using System.Linq;
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
        public IEnumerable<MpdOutput> Deserialize(SerializedResponse response)
        {
            var result = new List<MpdOutput>();

            // Strip out attributes so we can keep parsing the response by blocks of 4
            var strippedResult = response.ResponseValues.Where(kvp => kvp.Key != "attribute").ToList();

            for (var i = 0; i < strippedResult.Count; i+=4)
            {
                var outputId = int.Parse(strippedResult[i].Value);
                var outputName = strippedResult[i + 1].Value;
                var outputPlugin = strippedResult[i + 2].Value;
                var outputEnabled = strippedResult[i + 3].Value == "1";

                result.Add(new MpdOutput(outputId, outputName, outputPlugin, outputEnabled));
            }

            return result;
        }
    }
}