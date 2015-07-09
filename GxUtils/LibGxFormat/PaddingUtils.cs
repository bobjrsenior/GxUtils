using System;

namespace LibGxFormat
{
    static class PaddingUtils
    {
        /// <summary>
        /// Checks if the given number is a power of two.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <returns>true if it is a power of two, false otherwise.</returns>
        public static bool IsPowerOfTwo(int number)
        {
            // http://stackoverflow.com/a/600306
            return (number != 0) && ((number & (number - 1)) == 0);
        }

        /// <summary>
        /// Align data to the specified padding boundary.
        /// Padding must be a power of 2.
        /// </summary>
        /// <param name="number">The number to align.</param>
        /// <param name="padding">The padding boundary.</param>
        /// <returns>The number aligned to the padding boundary.</returns>
        public static int Align(int number, int padding)
        {
            if (!IsPowerOfTwo(padding))
                throw new ArgumentOutOfRangeException("padding", "padding must be a power of two.");

            return (number + padding - 1) & ~(padding - 1);
        }

        /// <summary>
        /// Align data to the specified padding boundary.
        /// Padding must be a power of 2.
        /// </summary>
        /// <param name="number">The number to align.</param>
        /// <param name="padding">The padding boundary.</param>
        /// <returns>The number aligned to the padding boundary.</returns>
        public static long Align(long number, int padding)
        {
            if (!IsPowerOfTwo(padding))
                throw new ArgumentOutOfRangeException("padding", "padding must be a power of two.");

            return (number + padding - 1) & ~(padding - 1);
        }
    }
}

