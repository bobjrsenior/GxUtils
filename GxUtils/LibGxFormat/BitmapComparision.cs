using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
            if (bitmap1.Width != bitmap2.Width || bitmap1.Height != bitmap2.Height)
                return false;
            if (bitmap1.Equals(bitmap2))
                return true;

            // https://codereview.stackexchange.com/a/39987

            bool equal = true;
            Rectangle rect = new Rectangle(0, 0, bitmap1.Width, bitmap1.Height);
            BitmapData bmp1Data = bitmap1.LockBits(rect, ImageLockMode.ReadOnly, bitmap1.PixelFormat);
            BitmapData bmp2Data = bitmap2.LockBits(rect, ImageLockMode.ReadOnly, bitmap2.PixelFormat);

            unsafe
            {
                // All images will be 32 bit ARGB, int is always 32 bits in C#
                int* img1Ptr = (int*)bmp1Data.Scan0.ToPointer();
                int* img2Ptr = (int*)bmp2Data.Scan0.ToPointer();
                
                for(int y = 0; y < rect.Height; y++)
                {
                    for(int x = 0; x < rect.Width; x++)
                    {
                        if(*img1Ptr != *img2Ptr)
                        {
                            equal = false;
                            break;
                        }
                        img1Ptr++;
                        img2Ptr++;
                    }
                }
            }
            bitmap1.UnlockBits(bmp1Data);
            bitmap2.UnlockBits(bmp2Data);
            return equal;
        }
    }
}
