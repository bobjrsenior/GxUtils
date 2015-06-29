using System;
using MiscUtil.IO;
using System.Text;

namespace LibGxFormat
{
    /// <summary>
    /// Extension methods for the class MiscUtil.IO.EndianBinaryWriter.
    /// </summary>
    static class EndianBinaryWriterExtensions
    {
        /// <summary>
        /// Align the stream offset to the specified boundary by writing zeroes.
        /// </summary>
        /// <param name="outputBinaryStream">The EndianBinaryWriter to align. Padding must be a power of 2.</param>
        /// <param name="padding">The padding boundary.</param>
        public static void Align(this EndianBinaryWriter outputBinaryStream, int padding)
        {
            if (outputBinaryStream == null)
                throw new ArgumentNullException("outputBinaryStream");

            long endOffset = PaddingUtils.Align(outputBinaryStream.BaseStream.Position, padding);
            int paddingAmount = (int)(endOffset - outputBinaryStream.BaseStream.Position);

            outputBinaryStream.Write(new byte[paddingAmount]);
        }

        /// <summary>
        /// Write a NUL-terminated ASCII string to a stream.
        /// </summary>
        /// <param name="outputBinaryStream">The stream to which to write the ASCII string.</param>
        /// <param name="str">The string to write.</param>
        public static void WriteAsciiString(this EndianBinaryWriter outputBinaryStream, string str)
        {
            if (outputBinaryStream == null)
                throw new ArgumentNullException("outputBinaryStream");
            if (str == null)
                throw new ArgumentNullException("str");

            outputBinaryStream.Write(Encoding.ASCII.GetBytes(str));
            outputBinaryStream.Write((byte)0);
        }
    }
}

