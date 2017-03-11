using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat
{
    static class BitmapComparision
    {

        /// <summary>
        /// Retrieve the value from a dictionary with a Bitmap as the key
        /// </summary>
        /// <param name="dict">>Dictionary with Bitmaps as a key and ints as values</param>
        /// <param name="bitmap">Key to look for</param>
        /// <returns>Value corresponding to the key</returns>
        public static int GetKeyFromBitmap(Dictionary<Bitmap, int> dict, Bitmap bitmap)
        {
            foreach (Bitmap bitmapKey in dict.Keys)
            {
                if (EqualBitmap(bitmap, bitmapKey))
                {
                    return dict[bitmapKey];
                }
            }
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Checks if a Bitmap is a key in a given dictionary
        /// </summary>
        /// <param name="dict">Dictionary with Bitmaps as a key and ints as values</param>
        /// <param name="bitmap">Key to look for</param>
        /// <returns>Whether or not the dictionary contains the key</returns>
        public static bool ContainsBitmap(Dictionary<Bitmap, int> dict, Bitmap bitmap)
        {
            foreach (Bitmap bitmapKey in dict.Keys)
            {
                if (EqualBitmap(bitmap, bitmapKey))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if two bitmaps are equal
        /// </summary>
        /// <param name="bitmap1"></param>
        /// <param name="bitmap2"></param>
        /// <returns>Returns true if both bitmaps are the same</returns>
        private static bool EqualBitmap(Bitmap bitmap1, Bitmap bitmap2)
        {
            // http://codereview.stackexchange.com/a/39989
            byte[] bitmap1Bytes;
            byte[] bitmap2Bytes;

            using (var mstream = new MemoryStream())
            {
                bitmap1.Save(mstream, bitmap1.RawFormat);
                bitmap1Bytes = mstream.ToArray();
            }

            using (var mstream = new MemoryStream())
            {
                bitmap2.Save(mstream, bitmap2.RawFormat);
                bitmap2Bytes = mstream.ToArray();
            }

            var bitmap164 = Convert.ToBase64String(bitmap1Bytes);
            var bitmap264 = Convert.ToBase64String(bitmap2Bytes);

            return string.Equals(bitmap164, bitmap264);
        }
    }
}
