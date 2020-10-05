using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MpcNET
{
    /// <summary>
    /// 
    /// </summary>
    public class MpdResponseReader : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public enum NextData
        {
#pragma warning disable CS1591
            Unknown,
            Eof,
            String,
            BinaryData
#pragma warning restore CS1591 
        }

        /// <summary>
        /// Length of the binary part of the response.
        /// </summary>
        public int binaryEnd;

        private Stream source;
        private Encoding encoding;
        private byte[] byteBuffer;
        private int bufferOffset;
        private int bufferEnd;
        private NextData nextData;
        private int binaryOffset;
        private char[] characterBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="encoding"></param>
        public MpdResponseReader(Stream source, Encoding encoding)
        {
            this.source = source;
            this.encoding = encoding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NextData ReportNextData()
        {
            if (nextData != NextData.Unknown)
            {
                return nextData;
            }

            if (!PopulateBufferIfNeeded(1))
            {
                return (nextData = NextData.Eof);
            }

            return (nextData = NextData.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            ReportNextData();

            if (nextData == NextData.Eof)
            {
                throw new EndOfStreamException();
            }
            else if (nextData != NextData.String)
            {
                throw new InvalidOperationException("Attempt to read non-string data as string");
            }

            if (characterBuffer == null)
            {
                characterBuffer = new char[4];
            }

            StringBuilder stringBuilder = new StringBuilder();
            Decoder decoder = encoding.GetDecoder(); 

            while (nextData == NextData.String)
            {
                byte b = byteBuffer[bufferOffset];

                if (b == '\n')
                {
                    // Analyze the obtained string to see if it's a binary data header
                    if (stringBuilder.ToString().StartsWith(Constants.Binary))
                    {
                        nextData = NextData.BinaryData;
                        binaryEnd = int.Parse(stringBuilder.ToString().Replace(Constants.Binary, ""));
                        binaryOffset = 0;
                    } else
                    {
                        nextData = NextData.Unknown;
                    }

                    bufferOffset++;
                    break;
                }
                else
                {
                    var decodedChars = decoder.GetChars(byteBuffer, bufferOffset++, 1, characterBuffer, 0);
                    if (decodedChars == 1)
                    {
                        stringBuilder.Append(characterBuffer[0]);
                    } 
                    else
                    {
                        stringBuilder.Append(characterBuffer, 0, decodedChars);
                    }

                    if (bufferOffset == bufferEnd && !PopulateBufferIfNeeded(1))
                    {
                        nextData = NextData.Eof;
                        break;
                    }
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int ReadBinaryData(byte[] buffer, int offset, int count)
        {
            ReportNextData();

            if (nextData == NextData.Eof)
            {
                throw new EndOfStreamException();
            }
            else if (nextData != NextData.BinaryData)
            {
                throw new InvalidOperationException("Attempt to read non-binary data as binary data");
            }

            if (count > binaryEnd - binaryOffset)
            {
                count = binaryEnd - binaryOffset;
                //throw new EndOfStreamException();
            }

            int bytesRead;

            if (bufferOffset < bufferEnd)
            {
                bytesRead = Math.Min(count, bufferEnd - bufferOffset);

                Array.Copy(byteBuffer, bufferOffset, buffer, offset, bytesRead);
                bufferOffset += bytesRead;
            }
            else if (count < byteBuffer.Length)
            {
                if (!PopulateBufferIfNeeded(1))
                {
                    throw new EndOfStreamException();
                }

                bytesRead = Math.Min(count, bufferEnd - bufferOffset);

                Array.Copy(byteBuffer, bufferOffset, buffer, offset, bytesRead);
                bufferOffset += bytesRead;
            }
            else
            {
                bytesRead = source.Read(buffer, offset, count);
            }

            binaryOffset += bytesRead;

            if (binaryOffset == binaryEnd)
            {
                nextData = NextData.Unknown;
            }

            return bytesRead;
        }

        private bool PopulateBufferIfNeeded(int minimumBytes)
        {
            if (byteBuffer == null)
            {
                byteBuffer = new byte[32768];
            }

            if (bufferEnd - bufferOffset < minimumBytes)
            {
                int shiftCount = bufferEnd - bufferOffset;

                if (shiftCount > 0)
                {
                    Array.Copy(byteBuffer, bufferOffset, byteBuffer, 0, shiftCount);
                }

                bufferOffset = 0;
                bufferEnd = shiftCount;

                while (bufferEnd - bufferOffset < minimumBytes)
                {
                    int bytesRead = source.Read(byteBuffer, bufferEnd, byteBuffer.Length - bufferEnd);

                    if (bytesRead == 0)
                    {
                        return false;
                    }

                    bufferEnd += bytesRead;
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Stream source = this.source;

            this.source = null;

            if (source != null)
            {
                source.Dispose();
            }
        }
    }
}
