using System;
using System.Collections.Generic;
using System.Text;

namespace MpcNET.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class MpdBinaryData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MpdBinaryData"/> class.
        /// </summary>
        /// <param name="size">The total size of the binary data requested</param>
        /// <param name="binary">The size of the data contained in this response</param>
        /// <param name="data">The data itself.</param>
        public MpdBinaryData(long size, long binary, byte[] data)
        {
            this.Size = size;
            this.Binary = binary;
            this.Data = data;
        }

        /// <summary>
        /// Gets the total size of the binary data requeste 
        /// </summary>
        public long Size { get; }
        /// <summary>
        /// Gets the size of the data contained in this response
        /// </summary>
        public long Binary { get; }
        /// <summary>
        /// Gets the data contained in this response
        /// </summary>
        public byte[] Data { get; }
    }
}
