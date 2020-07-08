// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecodersCommand.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Commands.Reflection
{
    using System.Collections.Generic;
    using MpcNET.Types;

    /// <summary>
    /// Print a list of decoder plugins, followed by their supported suffixes and MIME types.
    /// https://www.musicpd.org/doc/protocol/reflection_commands.html.
    /// </summary>
    public class DecodersCommand : IMpcCommand<IEnumerable<MpdDecoderPlugin>>
    {
        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => "decoders";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public IEnumerable<MpdDecoderPlugin> Deserialize(SerializedResponse response)
        {
            var result = new List<MpdDecoderPlugin>();

            var mpdDecoderPlugin = MpdDecoderPlugin.Empty;
            foreach (var line in response.ResponseValues)
            {
                if (line.Key.Equals("plugin"))
                {
                    if (mpdDecoderPlugin.IsInitialized)
                    {
                        result.Add(mpdDecoderPlugin);
                    }

                    mpdDecoderPlugin = new MpdDecoderPlugin(line.Value);
                }

                if (line.Key.Equals("suffix") && mpdDecoderPlugin.IsInitialized)
                {
                    mpdDecoderPlugin.AddSuffix(line.Value);
                }

                if (line.Key.Equals("mime_type") && mpdDecoderPlugin.IsInitialized)
                {
                    mpdDecoderPlugin.AddMediaType(line.Value);
                }
            }

            return result;
        }
    }
}