using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxModelViewer
{
    static class FlagHelper
    {
        public static string ERROR_VALUE = "CONF";

        /// <summary>
        /// A wrapper over float.parseFloat. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid float value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">float to store the result in</param>
        /// <returns></returns>
        public static bool parseFloat(string s, out float result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = -1;
                return false;
            }
            else
            {
                if (!float.TryParse(s, out result)) throw new InvalidOperationException(throwText);

                return true;
            }
        }

        /// <summary>
        /// A wrapper over byte.tryParse. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid byte value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">byte to store the result in</param>
        /// <returns></returns>
        public static bool parseByte(string s, out byte result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = 255;
                return false;
            }
            else
            {
                if (!byte.TryParse(s, out result)) throw new InvalidOperationException(throwText);

                return true;
            }
        }

        /// <summary>
        /// A wrapper over byte.tryParse. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid int32 value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">byte to store the result in</param>
        /// <returns></returns>
        public static bool parseHexToInt32(string s, out uint result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = uint.MaxValue;
                return false;
            }
            else
            {
                try
                {
                    result = Convert.ToUInt32(s, 16);
                }
                catch
                {
                    throw new InvalidOperationException(throwText);
                }
                return true;
            }
        }

        /// <summary>
        /// A wrapper over byte.tryParse. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid ushort value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">byte to store the result in</param>
        /// <returns></returns>
        public static bool parseHexToShort(string s, out ushort result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = ushort.MaxValue;
                return false;
            }
            else
            {
                try
                {
                    result = Convert.ToUInt16(s, 16);
                }
                catch
                {
                    throw new InvalidOperationException(throwText);
                }
                return true;
            }
        }

        /// <summary>
        /// A wrapper over byte.tryParse. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid byte value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">byte to store the result in</param>
        /// <returns></returns>
        public static bool parseHexToByte(string s, out byte result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = 255;
                return false;
            }
            else
            {
                try
                {
                    result = Convert.ToByte(s, 16);
                }
                catch
                {
                    throw new InvalidOperationException(throwText);
                }
                return true;
            }
        }
    }
}
