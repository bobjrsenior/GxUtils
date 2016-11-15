using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MiscUtil.IO;
using LibGxFormat.ModelRenderer;
using LibGxTexture;

/* The CMPR bug - Read this if you want to modify loading, saving or mipmapping.
 * ------------
 * First off, this is going to be pretty technical. Try to inform yourself about TPLs and CMPR beforehand.
 * This bug only affects F-Zero GX, Super Monkey Ball is not affected.
 *
 * Some details about the CMPR format:
 * - 4 bits per pixel.
 * - The size of a tile is 8x8 (so 64 pixels per tile, 32 bytes per tile).
 * - Each tile is divided in four 4x4 subtiles.
 *
 * The division of each tile in four 4x4 tiles is a format internal detail.
 * The right way is to just use 8x8 and let the decoder/encoder handle the rest.
 *
 * Normally, the way to calculate the size of a CMPR texture/mipmap is:
 * - Make the width and height multiples of 8.
 * - Calculate the size: width * height * (4 / 8).
 *
 * However, it looks like the programmers of F-Zero GX got misguided by the 4x4 subtiles detail.
 * This is how they implemented it:
 * - Make the width and height multiples of 4.
 * - Calculate the size: width * height * (4 / 8).
 * - Make the size multiple of 32.
 *
 * That last step was likely added as a workaround when they noticed that something wasn't right.
 * It has been checked with all the files of the game that this is indeed the way it works.
 *
 * ***
 *
 * The problem with this bug is that it underallocates the required buffer to store the texture and mipmaps.
 * (Side note: Square textures are not affected by this bug, since the two algorithms are identical for them).
 *
 * So how was this fixed? It's hard to tell, but it looks like the textures and mipmaps are encoded correctly.
 * But if they are encoded correctly, how do they fit into the buffer, which has been underallocated?
 *
 * It looks like at this point the programmers just decided to add yet another workaround.
 * It looks like they ended up checking if the image fits in the buffer, and if it doesn't, quit.
 *
 * The result is that some of the smallest mipmaps are not encoded.
 * I'm not sure how the game handles this, but this means we can't use them on OpenGL directly.
 *
 * (You can test those results using the dropLevel0() method on TplTexture).
 *
 * ***
 *
 * The way we solve it is:
 * - When reading, we create a buffer of "good size" bytes, but only read "bad size" bytes.
 * - When writing, we only write "bad size" bytes from the buffer.
 * - We let OpenGL generate the mipmaps.
 */

namespace LibGxFormat.Tpl
{
	/// <summary>A texture inside a TPL texture container.</summary>
	public class TplTexture : ITexture
	{
        static readonly IReadOnlyCollection<GxTextureFormat> supportedTextureFormatsBackingField = new List<GxTextureFormat> {
			GxTextureFormat.I4,
			GxTextureFormat.I8,
			GxTextureFormat.IA4,
			GxTextureFormat.RGB565,
			GxTextureFormat.RGB5A3,
			GxTextureFormat.RGBA8,
			GxTextureFormat.CMPR
		}.AsReadOnly();

		/// <summary>Get the list of formats supported for textures.</summary>
		public static IReadOnlyCollection<GxTextureFormat> SupportedTextureFormats
		{
			get
			{
                return supportedTextureFormatsBackingField;
			}
		}


		/// <summary>
		/// Value of the format field for textures with no levels defined.
		/// On textures with no levels defined, this will have nonsense values,
		/// which we save here in order to resave the original files perfectly.
		/// </summary>
		int formatRaw;

		/// <summary>
		/// Get the value of the format field for a texture with no levels defined.
		/// </summary>
		/// <value>The value of the format field for a texture with no levels defined.</value>
		public int FormatRaw
		{
			get
			{
				if (LevelCount != 0)
					throw new InvalidOperationException("Trying to get the raw format of a texture with levels defined.");

				return formatRaw;
			}
		}

		/// <summary>
        /// Format of the texture, or 0 if no texture levels are defined.
		/// </summary>
		GxTextureFormat format;

		/// <summary>Gets the format of the texture.</summary>
		public GxTextureFormat Format
		{
			get
			{
				if (LevelCount == 0)
					throw new InvalidOperationException ("Trying to get the format of a texture with no levels defined.");

				return format;
			}
		}

        /// <summary>Width of the main level of the texture, or 0 if no texture levels are defined</summary>
		int width;

        /// <summary>
        /// Get the width of the main level of the texture.
        /// </summary>
        public int Width
        {
            get
            {
                if (LevelCount == 0)
                    throw new InvalidOperationException("Trying to get the width of a texture with no levels defined.");

                return width;
            }
        }

        /// <summary>Height of the main level of the texture, or 0 if no texture levels are defined.</summary>
		int height;

        /// <summary>
        /// Get the height of the main level of the texture.
        /// </summary>
        public int Height
        {
            get
            {
                if (LevelCount == 0)
                    throw new InvalidOperationException("Trying to get the height of a texture with no levels defined.");

                return height;
            }
        }

		/// <summary>The texture data of each texture level (mipmap), in the encoded format.</summary>
		List<byte[]> encodedLevelData;

		/// <summary>
		/// Gets the number of texture levels.
		/// </summary>
		/// <value>The number of texture levels.</value>
		public int LevelCount
		{
			get
			{
				return encodedLevelData.Count;
			}
		}

        /// <summary>
        /// Checks if the texture is empty, that is, if it has no texture levels defined.
        /// </summary>
        /// <value>true if the texture is empty, that is, if it has no texture levels defined.</value>
        public bool IsEmpty
        {
            get
            {
                return LevelCount == 0;
            }
        }

		/// <summary>Create an empty (null) texture.</summary>
		public TplTexture()
		{
			DefineEmptyTexture(0);
		}

        /// <summary>
        /// Create a new texture from the given bitmap.
        /// </summary>
        /// <param name="bmp">The bitmap to build the texture from.</param>
        public TplTexture(GxTextureFormat format, Bitmap bmp)
        {
            DefineTextureFromBitmap(format, bmp);
        }

		/// <summary>
		/// Get the width of the specified texture level.
		/// </summary>
		/// <returns>The texture level.</returns>
		/// <param name="level">The width of the specified texture level.</param>
		public int WidthOfLevel(int level)
		{
			if (level < 0 || level >= LevelCount)
				throw new ArgumentOutOfRangeException("level");

			return width >> level;
		}

		/// <summary>
		/// Get the height of the specified texture level.
		/// </summary>
		/// <returns>The texture level.</returns>
		/// <param name="level">The width of the specified texture level.</param>
		public int HeightOfLevel(int level)
		{
			if (level < 0 || level >= encodedLevelData.Count)
				throw new ArgumentOutOfRangeException("level");

			return height >> level;
		}

        /// <summary>Define the texture as a texture with no texture levels.</summary>
        /// <param name="newFormatRaw">See definition of the FormatRaw property.</param>
		public void DefineEmptyTexture(int newFormatRaw)
		{
			formatRaw = newFormatRaw;
			format = (GxTextureFormat)0;
			width = 0;
			height = 0;
			encodedLevelData = new List<byte[]>();
		}

        /// <summary>Define the main level of the texture from the given properties.</summary>
		public void DefineMainLevel(GxTextureFormat newFormat, int newWidth, int newHeight,
			int newImageStride, byte[] newImageData)
		{
            if (SupportedTextureFormats.Contains(newFormat))
				throw new ArgumentOutOfRangeException("newFormat", "Unsupported format.");
			if (newWidth <= 0)
				throw new ArgumentOutOfRangeException("newWidth");
			if (newHeight <= 0)
                throw new ArgumentOutOfRangeException("newHeight");
            if (newImageStride < 0)
                throw new ArgumentOutOfRangeException("newImageStride");
            if (newImageStride < newWidth * 4)
                throw new ArgumentOutOfRangeException("newImageStride", "Stride is too small to contain a row of data.");
			if (newImageData == null)
				throw new ArgumentNullException("newImageData");

			format = newFormat;
			width = newWidth;
			height = newHeight;
            encodedLevelData = new List<byte[]>();

			DefineLevelData(0, newImageStride, newImageData);
		}

        /// <summary>Define the main level of the texture from the given bitmap.</summary>
        public void DefineMainLevelFromBitmap(GxTextureFormat newFormat, Bitmap bmp)
        {
            if (!SupportedTextureFormats.Contains(newFormat))
                throw new ArgumentOutOfRangeException("newFormat", "Unsupported format.");
            if (bmp == null)
                throw new ArgumentNullException("bmp");

            format = newFormat;
            width = bmp.Width;
            height = bmp.Height;
            encodedLevelData = new List<byte[]>();

            DefineLevelDataFromBitmap(0, bmp);
        }

		/// <summary>
		/// Create or replace the specified texture level from the specified image data.
		/// New texture levels must be created in order.
		/// </summary>
		/// <param name="level">The level of the texture to create or replace.</param>
		/// <param name="newImageDataStride">The stride (number of bytes per row) of the new image data.</param>
		/// <param name="newImageData">The new image data for the level.</param>
		public void DefineLevelData(int level, int newImageDataStride, byte[] newImageData)
		{
			if (level > LevelCount) // We allow to either replace an existing level or to generate the next level
				throw new ArgumentOutOfRangeException("level");
            if (newImageDataStride < 0)
                throw new ArgumentOutOfRangeException("newImageDataStride");
			if (newImageData == null)
				throw new ArgumentNullException("newImageData");

			// Check that this texture level can be defined (size is not too small)
            // This checks that width and height can be divided evenly by 2^level
			if ((width & ((1 << level) - 1)) != 0 ||
				(height & ((1 << level) - 1)) != 0)
			{
                throw new ArgumentOutOfRangeException("level", "Level is too low for the image dimensions.");
			}

			int levelWidth = width >> level;
			int levelHeight = height >> level;

            if (newImageDataStride < levelWidth * 4)
                throw new ArgumentOutOfRangeException("newImageDataStride", "Stride is too small to contain a row of data.");

			// Adding a new mipmap?
			if (level == LevelCount)
				encodedLevelData.Add(new byte[CalculateSizeOfLevel(level)]);

			// Encode
            GxTextureFormatCodec.GetCodec(format).EncodeTexture(
                newImageData, 0, levelWidth, levelHeight, newImageDataStride,
                encodedLevelData[level], 0, null, 0);
		}

        /// <summary>
        /// Create or replace the specified texture level from the specified bitmap.
        /// New texture levels must be created in order.
        /// </summary>
        public void DefineLevelDataFromBitmap(int level, Bitmap bmp)
        {
            if (level > LevelCount) // We allow to either replace an existing level or to generate the next level
                throw new ArgumentOutOfRangeException("level");
            if (bmp == null)
                throw new ArgumentNullException("bmp");

            // Check that the bitmap is of the appropiate size to replace this texture
            int levelWidth = width >> level;
            int levelHeight = height >> level;

            if (bmp.Width != levelWidth ||
                bmp.Height != levelHeight)
            {
                throw new ArgumentOutOfRangeException("bmp", "Bitmap doesn't have the correct dimensions to replace this level.");
            }

            // Extract the BMP data as an array of ARGB8 pixels
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] levelData = new byte[bmpData.Height * bmpData.Stride];
            Marshal.Copy(bmpData.Scan0, levelData, 0, bmpData.Height * bmpData.Stride);
            bmp.UnlockBits(bmpData);

            // LockBits gives us the data as ARGB when seen as uint,
            // so we need to shuffle the bitmap data accordingly to go to RGBA
            SwapRgbaFromToArgbAsUint(levelData, bmpData.Width, bmpData.Height, bmpData.Stride);
            
            // Encode the data to the given format
            DefineLevelData(level, bmpData.Stride, levelData);
        }

        /// <summary>
        /// Defines the texture from a bitmap.
        /// All texture levels will be generated until the texture size is no longer divisible by two.
        /// </summary>
        /// <param name="format">The format to encode the new texture as.</param>
        /// <param name="bmp">The bitmap that will define the texture.</param>
        public void DefineTextureFromBitmap(GxTextureFormat format, Bitmap bmp)
        {
            if (!SupportedTextureFormats.Contains(format))
                throw new ArgumentOutOfRangeException("format", "Unsupported format.");
            if (bmp == null)
                throw new ArgumentNullException("bmp");

            // Define all possible texture levels until the size
            // of the texture can no longer be divided by two
            int currentWidth = bmp.Width, currentHeight = bmp.Height;
            for (int mipmapLevel = 0; true; mipmapLevel++)
            {
                if (mipmapLevel == 0)
                {
                    DefineMainLevelFromBitmap(format, bmp);
                }
                else
                {
                    DefineLevelDataFromBitmap(mipmapLevel, new Bitmap(bmp, currentWidth, currentHeight));
                }

                if ((currentWidth % 2) != 0 || (currentHeight % 2) != 0)
                    break;

                currentWidth /= 2;
                currentHeight /= 2;
            }
        }

		/// <summary>
		/// Decodes the specified level of the encoded texture to an array of RGBA8 pixels.
		/// </summary>
        /// <param name="level">The level of the texture to decode.</param>
        /// <param name="desiredStride">Desired stride (number of bytes per scanline) of the result.</param>
        /// <returns>An array with the RGBA8 data of the level (with no extra row padding).</returns>
        public byte[] DecodeLevelToRGBA8(int level, int desiredStride)
		{
            if (level < 0 || level >= LevelCount)
				throw new ArgumentOutOfRangeException("level");
            if (desiredStride < 0)
                throw new ArgumentOutOfRangeException("desiredStride");

			int levelWidth = WidthOfLevel(level), levelHeight = HeightOfLevel(level);

            if (desiredStride < levelWidth * 4)
                throw new ArgumentOutOfRangeException("desiredStride", "Stride is too small to contain a row of data.");

			// Decode texture as RGBA8 (GxTextureDecode format)
            byte[] decodedData = new byte[levelHeight * desiredStride];
            GxTextureFormatCodec.GetCodec(format).DecodeTexture(decodedData, 0,
				levelWidth, levelHeight, desiredStride,
                encodedLevelData[level], 0, null, 0);
            return decodedData;
		}

        /// <summary>
        /// Decodes the specified level of the encoded texture to a bitmap.
        /// </summary>
        /// <param name="level">The level of the texture to decode.</param>
        /// <returns>A bitmap corresponding to the texture data of the level.</returns>
        public Bitmap DecodeLevelToBitmap(int level)
        {
            if (level < 0 || level >= LevelCount)
                throw new ArgumentOutOfRangeException("level");

            int levelWidth = WidthOfLevel(level), levelHeight = HeightOfLevel(level);

            // Create the new bitmap where the texture will be decoded
            Bitmap bmp = new Bitmap(levelWidth, levelHeight);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, levelWidth, levelHeight),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // Decode the data of the texture level
            byte[] levelData = DecodeLevelToRGBA8(level, bmpData.Stride);

            // LockBits expects to see data as ARGB when seen as uint,
            // so we need to shuffle the bitmap data accordingly since we have RGBA
            SwapRgbaFromToArgbAsUint(levelData, bmpData.Width, bmpData.Height, bmpData.Stride);

            // Copy the decode data over the bitmap data
            Marshal.Copy(levelData, 0, bmpData.Scan0, bmpData.Stride * levelHeight);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        /// <summary>
        /// Swaps the given pixel array from/to RGBA pixels (when observed as a byte array)
        /// and ARGB pixels (when observed as an uint). ARGB pixels as uint is the format used by the Bitmap class.
        /// </summary>
        /// <param name="pixels">The array of pixels to swap.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="stride">The number of bytes between each row in the input array.</param>
        private static void SwapRgbaFromToArgbAsUint(byte[] pixels, int width, int height, int stride)
        {
            if (BitConverter.IsLittleEndian)
            {
                for (int y = 0, pos = 0; y < height; y++, pos += stride - width * 4)
                {
                    for (int x = 0; x < width; x++, pos += 4)
                    {
                        byte b = pixels[pos + 0];
                        byte g = pixels[pos + 1];
                        byte r = pixels[pos + 2];
                        byte a = pixels[pos + 3];

                        pixels[pos + 0] = r;
                        pixels[pos + 1] = g;
                        pixels[pos + 2] = b;
                        pixels[pos + 3] = a;
                    }
                }
            }
            else
            {
                for (int y = 0, pos = 0; y < height; y++, pos += stride - width * 4)
                {
                    for (int x = 0; x < width; x++, pos += 4)
                    {
                        byte a = pixels[pos + 0];
                        byte r = pixels[pos + 1];
                        byte g = pixels[pos + 2];
                        byte b = pixels[pos + 3];

                        pixels[pos + 0] = r;
                        pixels[pos + 1] = g;
                        pixels[pos + 2] = b;
                        pixels[pos + 3] = a;
                    }
                }
            }
        }

		/// <summary>
		/// Calculates the size of a level of the texture.
		/// </summary>
		/// <param name="level">The level of the texture.</param>
		/// <param name="replicateCmprBug">true to replicate the F-Zero GX CMPR encoding bug.</param>
		/// <returns>The size of the encoded data in the specified level.</returns>
		private int CalculateSizeOfLevel(int level, bool replicateCmprBug = false)
		{
            // Here we allow also to specify the "next" level for easier implementation
            // of the methods that encode the new texture
			if (level < 0 || level > LevelCount)
				throw new ArgumentOutOfRangeException("level");

            int levelWidth = width >> level, levelHeight = height >> level;

			// Hack: CMPR sizes are not calculated correctly, replicate the bug
			if (replicateCmprBug && format == GxTextureFormat.CMPR)
			{
				int w = PaddingUtils.Align(levelWidth, 4); // Align to 4 (should really be 8)
				int h = PaddingUtils.Align(levelHeight, 4); // Align to 4 (should really be 8)
				int sz = (w * h * 4) / 8; // CMPR is 4 bits per pixel
				int szpad = PaddingUtils.Align(sz, 32); // Align to 32 (this should normally not be needed)
				return szpad;
			}

            return GxTextureFormatCodec.GetCodec(format).CalcTextureSize(levelWidth, levelHeight);
		}

		/// <summary>
		/// Reads a texture with the specified characteristics from a binary stream.
		/// </summary>
		internal void LoadTextureData(EndianBinaryReader input, GxGame game, GxTextureFormat format, int width, int height, int levelCount)
		{
			if (!SupportedTextureFormats.Contains(format))
				throw new InvalidTplFileException("Unsupported texture format.");

			this.format = format;
			this.width = width;
			this.height = height;

			for (int level = 0; level < levelCount; level++)
			{
				byte[] levelData = new byte[CalculateSizeOfLevel(level)];
				input.Read(levelData, 0, CalculateSizeOfLevel(level, (game == GxGame.FZeroGX)));
				encodedLevelData.Add(levelData);
			}
		}

		/// <summary>
		/// Writes this texture to a binary stream.
		/// </summary>
		internal void SaveTextureData(EndianBinaryWriter output, GxGame game)
		{

            if (LevelCount != 0 && game == GxGame.SuperMonkeyBallDX)
            {
                List<byte> texHeader = new List<byte>();
                switch (format)
                {
                    case GxTextureFormat.CMPR:
                        texHeader.Add(0x0C);
                        texHeader.Add(0x00);
                        texHeader.Add(0x00);
                        texHeader.Add(0x00);
                        break;

                    case GxTextureFormat.I8:
                        texHeader.Add(0x1A);
                        texHeader.Add(0x00);
                        texHeader.Add(0x00);
                        texHeader.Add(0x00);
                        break;

                    default:
                        texHeader.Add(0x0C);
                        texHeader.Add(0x00);
                        texHeader.Add(0x00);
                        texHeader.Add(0x00);
                        break;
                }
                // Width
                texHeader.Add((byte)WidthOfLevel(0));
                texHeader.Add((byte)(WidthOfLevel(0) >> 8));
                // Padding
                texHeader.Add(0);
                texHeader.Add(0);

                // Height
                texHeader.Add((byte)HeightOfLevel(0));
                texHeader.Add((byte)(HeightOfLevel(0) >> 8));
                texHeader.Add(0);
                texHeader.Add(0);

                // Five
                texHeader.Add(5);
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);

                // 0 for uncompressed
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);

                // Data Length
                int levelSize = 0;
                for (int level = 0; level < LevelCount; level++)
                {
                    levelSize += CalculateSizeOfLevel(level, (game == GxGame.FZeroGX));
                }
                texHeader.Add((byte)levelSize);
                texHeader.Add((byte)(levelSize >> 8));
                texHeader.Add((byte)(levelSize >> 16));
                texHeader.Add((byte)(levelSize >> 24));

                // Data length + a little (0 if uncompressed)
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);

                // Zero/Padding?
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);
                texHeader.Add(0);

                output.Write(texHeader.ToArray(), 0, texHeader.Count);

            }

            for (int level = 0; level < LevelCount; level++)
			{
				output.Write(encodedLevelData[level], 0, CalculateSizeOfLevel(level, (game == GxGame.FZeroGX)));
			}
		}

		/// Gets the size of the texture when written to a binary stream.
		internal int SizeOfTextureData(GxGame game)
		{
			int size = 0;
            if(game == GxGame.SuperMonkeyBallDX)
            {
                size += 0x20;
            }
			for (int level = 0; level < LevelCount; level++)
			{
				size += CalculateSizeOfLevel(level, (game == GxGame.FZeroGX));
			}
			return size;
		}
	}
}

