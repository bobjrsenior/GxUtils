using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxLoadSaveTestApp
{
    static class ByteArrayUtils
    {
        /// <summary>
        /// Check if two byte arrays contain the same data.
        /// </summary>
        /// <param name="first">The first array to compare.</param>
        /// <param name="second">The second array to compare.</param>
        /// <returns>true if they are equal, false otherwise.</returns>
        public static bool ByteArrayDataEquals(byte[] first, byte[] second)
        {
            // Both point to the same reference or are null -> equal
            if (first == second)
                return true;

            // One is null and the other isn't -> different
            if (first == null || second == null)
                return false;

            // If they aren't of the same length -> different
            if (first.Length != second.Length)
                return false;

            // Compare the array data byte per byte
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }
    }
}
