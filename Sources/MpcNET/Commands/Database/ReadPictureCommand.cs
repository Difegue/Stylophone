using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MpcNET.Commands.Database
{
    /// <summary>
    /// Gets the album art for the given song, using ID3 metadata.
    /// https://www.musicpd.org/doc/html/protocol.html#the-music-database
    /// </summary>
    public class ReadPictureCommand : IMpcCommand<MpdBinaryData>
    {
        private readonly string path;
        private readonly long binaryOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadPictureCommand"/> class.
        /// </summary>
        /// <param name="path">The URI.</param>
        /// <param name="offset">Binary data offset if needed</param>
        public ReadPictureCommand(string path, long offset = 0)
        {
            this.path = path;
            this.binaryOffset = offset;
        }

        /// <summary>
        /// Serializes the command.
        /// </summary>
        /// <returns>
        /// The serialize command.
        /// </returns>
        public string Serialize() => $"readpicture \"{path}\" {binaryOffset}";

        /// <summary>
        /// Deserializes the specified response text pairs.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The deserialized response.
        /// </returns>
        public MpdBinaryData Deserialize(SerializedResponse response)
        {
            if (response.ResponseValues.Count == 0)
                return null;

            var totalSize = long.Parse(response.ResponseValues.Where(kvp => kvp.Key == "size").Select(kvp => kvp.Value).First()); 
            var payloadSize = long.Parse(response.ResponseValues.Where(kvp => kvp.Key == "binary").Select(kvp => kvp.Value).First());

            return new MpdBinaryData(totalSize, payloadSize, response.BinaryData);
        }
    }
}
