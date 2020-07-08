using System;
using System.Collections.Generic;
using System.Text;

namespace MpcNET
{
    /// <summary>
    /// 
    /// </summary>
    public class SerializedResponse
    {
        /// <summary>
        /// Response values.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> ResponseValues {get; set;}
        /// <summary>
        /// Binary Data that can be present in the response.
        /// </summary>
        public byte[] BinaryData { get; set; }

    }
}
