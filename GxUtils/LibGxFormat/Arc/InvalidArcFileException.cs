using System;
using System.Runtime.Serialization;

namespace LibGxFormat.Arc
{
    /// <summary>
    /// Thrown when an invalid .ARC file is read/written.
    /// </summary>
    public class InvalidArcFileException : Exception
    {
        public InvalidArcFileException()
        {
        }

        public InvalidArcFileException(string message)
            : base(message)
        {
        }

        public InvalidArcFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidArcFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
