using System;
using System.Linq;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using LibGxTexture;
using LibGxFormat.ModelLoader;
using System.Collections.Generic;
using System.Drawing;

namespace LibGxFormat.Tpl
{
	/// <summary>
	/// A container of TPL textures.
    /// Be careful, because a TPL container can contain textures with no levels defined,
    /// i.e. textures that have no associated texture data.
	/// </summary>
	public class Tpl : NonNullableCollection<TplTexture>
	{
		/// <summary>
		/// Create an empty Tpl texture container.
		/// </summary>
		public Tpl()
		{
		}

        /// <summary>
        /// Create a TPL texture file from the specified model.
        /// </summary>
        /// <param name="model">The model to create the TPL file from.</param>
        /// <param name="textureIndexMapping">The correspondence between textures images in the model and the generated TPL texture indices.</param>
        public Tpl(ObjMtlModel model, GxInterpolationFormat intFormat, int numMipmaps, out Dictionary<Bitmap, int> textureIndexMapping)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            // Gather all material definitions in the model
            IEnumerable<ObjMtlMaterial> allMaterials = model.Objects
                .SelectMany(o => o.Value.Meshes).Select(m => m.Material);

            Dictionary<Bitmap, int> textureIndexMappingInt = new Dictionary<Bitmap, int>();

            foreach (ObjMtlMaterial mat in allMaterials)
            {
                // Create and add texture for diffuse map
                if (mat.DiffuseTextureMap != null && !textureIndexMappingInt.ContainsKey(mat.DiffuseTextureMap))
                {
                    int textureIndex = Count;
                    TplTexture texture = new TplTexture(GxTextureFormat.CMPR, intFormat, numMipmaps, mat.DiffuseTextureMap);
                    Add(texture);
                    textureIndexMappingInt.Add(mat.DiffuseTextureMap, textureIndex);
                }
            }

            // Replace the 'out' variable at the end so it does not get
            // modified if an exception 
            textureIndexMapping = textureIndexMappingInt;
        }

		/// <summary>
		/// Create a Tpl texture container from a .TPL file.
		/// </summary>
		/// <param name="inputStream">The input stream that contains the .TPL file.</param>
		/// <param name="game">The game from which the .TPL file is.</param>
		public Tpl(Stream inputStream, GxGame game)
		{
			if (inputStream == null)
				throw new ArgumentNullException("inputStream");
			if (!Enum.IsDefined(typeof(GxGame), game))
				throw new ArgumentOutOfRangeException("game");

            if(game == GxGame.SuperMonkeyBallDX)
            {
                Load(new EndianBinaryReader(EndianBitConverter.Little, inputStream), game);
            }
            else
            {
                Load(new EndianBinaryReader(EndianBitConverter.Big, inputStream), game);
            }
			
		}

        /// <summary>
        /// Header of a TPL texture in a file, for loading purposes only.
        /// </summary>
        private struct TextureHeader
        {
            public int FormatRaw;
            public int Offset;
            public int Width;
            public int Height;
            public int LevelCount;
        }

        private void Load(EndianBinaryReader input, GxGame game)
        {
            if(game == GxGame.SuperMonkeyBallDX)
            {
                input.ReadInt32();
            }
            int numTextures = input.ReadInt32();

            // Load texture definition headers
            TextureHeader[] texHdr = new TextureHeader[numTextures];
            for (int i = 0; i < numTextures; i++)
            {
                texHdr[i].FormatRaw = input.ReadInt32();
                texHdr[i].Offset = input.ReadInt32();
                texHdr[i].Width = Convert.ToInt32(input.ReadUInt16());
                texHdr[i].Height = Convert.ToInt32(input.ReadUInt16());
                texHdr[i].LevelCount = Convert.ToInt32(input.ReadUInt16());
                UInt16 check = input.ReadUInt16();
                if ((game != GxGame.SuperMonkeyBallDX && check != 0x1234) || (game == GxGame.SuperMonkeyBallDX && check != 0x3412))
                    throw new InvalidTplFileException("Invalid texture header (Field @0x0E).");
            }

            // Load textures data
            for (int i = 0; i < numTextures; i++)
            {
                TplTexture tex = new TplTexture();

                if (texHdr[i].Offset != 0 && texHdr[i].Width != 0 &&
                    texHdr[i].Height != 0 && texHdr[i].LevelCount != 0) // Texture with defined levels
                {
                    if (game != GxGame.SuperMonkeyBallDX && !Enum.IsDefined(typeof(GxTextureFormat), texHdr[i].FormatRaw))
                        throw new InvalidTplFileException("Invalid texture header (invalid format.");
                    if (game == GxGame.SuperMonkeyBallDX)
                    {
                        input.BaseStream.Position = texHdr[i].Offset;
                        int formatRaw = input.ReadInt32();
                        switch (formatRaw)
                        {
                            case 0x0C:
                                texHdr[i].FormatRaw = 0x0E;
                            break;
                            case 0x1A:
                                texHdr[i].FormatRaw = 0x01;
                            break;
                            case 0x0E:
                                texHdr[i].FormatRaw = 0x05;
                            break;
                            default:
                                int x = 5;
                            break;
                        }
                        texHdr[i].Width = input.ReadInt32();
                        texHdr[i].Height = input.ReadInt32();
                        texHdr[i].LevelCount = input.ReadInt32();
                        // Compressed?
                        input.ReadInt32();
                        // If uncompressed, data length?
                        input.ReadInt32();
                        // Some other length (for compressed)?
                        input.ReadInt32();
                        // Zero?
                        input.ReadInt32();

                        if (!Enum.IsDefined(typeof(GxTextureFormat), texHdr[i].FormatRaw))
                            throw new InvalidTplFileException("Invalid texture header (invalid format.");
                    }
                    else
                    {
                        input.BaseStream.Position = texHdr[i].Offset;
                    }
                    tex.LoadTextureData(input, game, (GxTextureFormat)texHdr[i].FormatRaw,
                            texHdr[i].Width, texHdr[i].Height, texHdr[i].LevelCount);
                }
                else if (texHdr[i].Offset == 0 && texHdr[i].Width == 0 &&
                    texHdr[i].Height == 0 && texHdr[i].LevelCount == 0) // Texture with no defined levels
                {
                    tex.DefineEmptyTexture(texHdr[i].FormatRaw);
                }
                else
                {
                    throw new InvalidTplFileException("Invalid texture header (invalid combination of fields).");
                }

                Add(tex);
            }
        }

        /// <summary>
        /// Calculate the size of the TPL when written to a file.
        /// </summary>
        /// <param name="game">The game from which the .TPL file is.</param>
        /// <returns>The size of the TPL when written to a file.</returns>
        public int SizeOf(GxGame game)
        {
            return SizeOfHeaderEntries(game) + SizeOfTextureData(game);
        }

        private int SizeOfHeaderEntries(GxGame game)
        {
            if(game == GxGame.SuperMonkeyBallDX)
            {
                return PaddingUtils.Align((8 + (4 + 4 + 2 + 2 + 2 + 2) * Count), 0x20);
            }
            else
            {
                return PaddingUtils.Align(4 + (4 + 4 + 2 + 2 + 2 + 2) * Count, 0x20);
            }
            
        }

        private int SizeOfTextureData(GxGame game)
        {
            // No need to worry about textures with no levels, they have size zero
            return Items.Sum(t => t.SizeOfTextureData(game));
        }

		/// <summary>
		/// Save a Tpl texture container to a .TPL file.
		/// </summary>
		/// <param name="outputStream">The input stream to which to write the .TPL file.</param>
		/// <param name="game">The game from which the .TPL file is.</param>
		public void Save(Stream outputStream, GxGame game)
		{
			if (outputStream == null)
				throw new ArgumentNullException("outputStream");
			if (!Enum.IsDefined(typeof(GxGame), game))
				throw new ArgumentOutOfRangeException("game");
            
            if(game == GxGame.SuperMonkeyBallDX)
            {
                Save(new EndianBinaryWriter(EndianBitConverter.Little, outputStream), game);
            }
            else
            {
                Save(new EndianBinaryWriter(EndianBitConverter.Big, outputStream), game);
            }
			
		}

        private void Save(EndianBinaryWriter output, GxGame game)
        {
            if (game == GxGame.SuperMonkeyBallDX)
            {
                output.Write('X');
                output.Write('T');
                output.Write('P');
                output.Write('L');
            }

            output.Write(Count);

            // Write texture definition headers
            int beginDataOffset = SizeOfHeaderEntries(game);
            int currentDataOffset = beginDataOffset;

            foreach (TplTexture tex in Items)
            {
                if (tex.LevelCount != 0)
                {
                    output.Write((int)tex.Format);
                    output.Write(currentDataOffset);
                    output.Write(Convert.ToUInt16(tex.WidthOfLevel(0)));
                    output.Write(Convert.ToUInt16(tex.HeightOfLevel(0)));
                    output.Write(Convert.ToUInt16(tex.LevelCount));
                }
                else
                {
                    output.Write(tex.FormatRaw);
                    output.Write((int)0);
                    output.Write((ushort)0);
                    output.Write((ushort)0);
                    output.Write((ushort)0);
                }
                if(game == GxGame.SuperMonkeyBallDX)
                {
                    output.Write((ushort)0x3412);
                }
                else
                {
                    output.Write((ushort)0x1234);
                }

                currentDataOffset += tex.SizeOfTextureData(game);
                
            }

            int paddingAmount = beginDataOffset - Convert.ToInt32(output.BaseStream.Position);
            for (int i = 0; i < paddingAmount; i++)
                output.Write((byte)i); // Curious padding pattern of 0x00, 0x01, 0x02, 0x03, ...

            // Write texture data
            foreach (TplTexture tex in Items)
            {
                // No need to worry about textures with no levels, they have size zero
                tex.SaveTextureData(output, game);
            }
        }
	}
}

