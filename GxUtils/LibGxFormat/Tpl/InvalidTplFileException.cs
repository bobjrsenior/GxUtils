using System;
using System.Runtime.Serialization;

namespace LibGxFormat.Tpl
{
    /// <summary>
    /// Thrown when an invalid .TPL file is read/written.
    /// </summary>
    public class InvalidTplFileException : Exception
    {
        public InvalidTplFileException()
        {
        }

        public InvalidTplFileException(string message)
            : base(message)
        {
        }

        public InvalidTplFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidTplFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

