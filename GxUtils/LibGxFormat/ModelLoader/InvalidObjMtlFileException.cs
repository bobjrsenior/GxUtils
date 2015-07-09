using System;
using System.Runtime.Serialization;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// Thrown when an invalid .OBJ/.MTL file is read/written.
    /// </summary>
    public class InvalidObjMtlFileException : Exception
    {
        public InvalidObjMtlFileException()
        {
        }

        public InvalidObjMtlFileException(string message)
            : base(message)
        {
        }

        public InvalidObjMtlFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidObjMtlFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }


}
