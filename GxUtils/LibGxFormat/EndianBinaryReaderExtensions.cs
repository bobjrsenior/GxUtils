using System;
using System.Collections.Generic;
using System.Text;
using MiscUtil.IO;

namespace LibGxFormat
{
    /// <summary>
    /// Extension methods for MiscUtil.IO.EndianBinaryReader.
    /// </summary>
    static class EndianBinaryReaderExtensions
    {
        /// <summary>
        /// Read a NUL-terminated ASCII string from a stream.
        /// </summary>
        /// <param name="inputBinaryStream">The stream from which to read the ASCII string.</param>
        /// <returns>The ASCII string value.</returns>
        public static string ReadAsciiString(this EndianBinaryReader inputBinaryStream)
        {
            if (inputBinaryStream == null)
                throw new ArgumentNullException("inputBinaryStream");

            List<byte> bytes = new List<byte>();
            for (byte b = inputBinaryStream.ReadByte(); b != 0; b = inputBinaryStream.ReadByte())
                bytes.Add(b);
            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
