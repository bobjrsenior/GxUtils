using System;
using System.Runtime.Serialization;

namespace GxExpander
{
    /// <summary>
    /// Thrown when an error happens on the GxExpander class.
    /// </summary>
    public class GxExpanderException : Exception
    {
        public GxExpanderException()
            : base()
        {
        }

        public GxExpanderException(string message)
            : base(message)
        {
        }

        public GxExpanderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public GxExpanderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
