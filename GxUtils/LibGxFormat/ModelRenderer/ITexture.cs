
using System.Drawing;
namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Generic interface to be implemented by textures.
    /// </summary>
    public interface ITexture
    {
        /// <summary>
        /// Get the number of texture levels of the texture (may be 0, in which case the texture contains no data).
        /// </summary>
        int LevelCount
        {
            get;
        }

        /// <summary>
        /// Get the width of the main level of the texture.
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// Get the height of the main level of the texture.
        /// </summary>
        int Height
        {
            get;
        }

        /// <summary>
        /// Get the data of the specified texture level as an array of RGBA8 bytes (4 bytes per pixel).
        /// </summary>
        /// <param name="level">The level of the texture to decode.</param>
        /// <param name="desiredStride">The stride (number of bytes per scanline) that the result will have.</param>
        /// <returns>The decoded texture level data.</returns>
        byte[] DecodeLevelToRGBA8(int level, int desiredStride);

        /// <summary>
        /// Get a bitmap corresponding to the specified texture level.
        /// </summary>
        /// <param name="level">The level of the texture to decode.</param>
        /// <returns>The Bitmap corresponding to the specified texture level.</returns>
        Bitmap DecodeLevelToBitmap(int level);
    };
}
