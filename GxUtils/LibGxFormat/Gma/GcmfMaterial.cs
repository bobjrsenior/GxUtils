using LibGxFormat.ModelLoader;
using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// A Gcmf material entry contains the definition of a single object material.
    /// </summary>
    public class GcmfMaterial
    {
        // OR of all flags of all GMA files: 0x5AFFFF
        // Bits 0x0000000C - Texture Wrap S (0 - Clamp, 1 - Repeat, 2 - Mirror)
        // Bits 0x00000030 - Texture Wrap T (0 - Clamp, 1 - Repeat, 2 - Mirror)
        // Bits 0x00000040 - Anisotropic filtering? (Seems to be VERY related, but sometimes anisotropyLevel is set without this..)
        // Bits 0x00000700 - Related to mipmaping (number/proportion of mipmap levels to use?)
        //                  If set to zero, shows as XXX & MIPMAP NEAR - XXX
        //                  With increasing values, more mipmaps are shown big in the debug menu.
        // Bits 0x00000080 - Near mag & min filter (Shows as NEAR & MIPMAP XXX - NEAR in the debug menu)
        // Bits 0x00020000 - Makes some textures display a scrolling animation. Used for example for the booster pad animation.
        public uint Flags { get; set; }
        /// <summary>
        /// Index of the texture in the .TPL file.
        /// </summary>
        public ushort TextureIdx { get; set; }
        public byte Unk6 { get; set; }
        public byte AnisotropyLevel { get; set; } // ANISO level (0 = ANISO 1, 1 = ANISO 2, 2 = ANISO 4)
        public ushort UnkC { get; set; }
        public uint Unk10 { get; set; }

        public GcmfMaterial()
        {
            Flags = 0x7D4;
            TextureIdx = ushort.MaxValue;
            Unk6 = 0;
            AnisotropyLevel = 0;
            UnkC = 0x2E00;
            Unk10 = 0x00000030;
        }

        public GcmfMaterial(ObjMtlMaterial mtl, Dictionary<Bitmap, int> modelTextureMapping)
            : this()
        {
            Flags = 0x7D4; // TODOXXX
            if (mtl.DiffuseTextureMap != null)
            {
                if (!BitmapComparision.ContainsBitmap(modelTextureMapping, mtl.DiffuseTextureMap))
                    throw new InvalidOperationException("Diffuse texture map not found in modelTextureMapping.");
                TextureIdx = Convert.ToUInt16(BitmapComparision.GetKeyFromBitmap(modelTextureMapping, mtl.DiffuseTextureMap));
            }
        }

        /// <summary>
        /// Set up this Gcmf material definition in the given material renderer.
        /// </summary>
        internal void Render(IRenderer renderer, int materialIndex)
        {
            TextureWrapMode wrapS, wrapT;

            switch ((Flags >> 2) & 0x03)
            {
                case 0: wrapS = TextureWrapMode.ClampToEdge; break;
                case 1: wrapS = TextureWrapMode.Repeat; break;
                case 2: wrapS = TextureWrapMode.MirroredRepeat; break;
                default: throw new InvalidGmaFileException("Invalid wrapS mode.");
            }

            switch ((Flags >> 4) & 0x03)
            {
                case 0: wrapT = TextureWrapMode.ClampToEdge; break;
                case 1: wrapT = TextureWrapMode.Repeat; break;
                case 2: wrapT = TextureWrapMode.MirroredRepeat; break;
                default: throw new InvalidGmaFileException("Invalid wrapT mode.");
            }

            renderer.DefineMaterial(materialIndex, TextureIdx, wrapS, wrapT);
        }

        /// <summary>
        /// Load a Gcmf material definition from the given .GMA stream.
        /// </summary>
        internal void Load(EndianBinaryReader input, int materialIndex)
        {
            Flags = input.ReadUInt32();
            TextureIdx = input.ReadUInt16();
            Unk6 = input.ReadByte();
            AnisotropyLevel = input.ReadByte();
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfMaterial[0x08] == 0");
            UnkC = input.ReadUInt16();
            ushort checkMaterialIndex = input.ReadUInt16();
            if (checkMaterialIndex != materialIndex)
                throw new InvalidGmaFileException("Expected GcmfMaterial[0x0E] to match the material index.");
            Unk10 = input.ReadUInt32();
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfMaterial[0x14] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfMaterial[0x18] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfMaterial[0x1C] == 0");
        }

        /// <summary>
        /// Get the size of this Gcmf material definition when written to a .GMA stream.
        /// </summary>
        internal int SizeOf()
        {
            return 0x20;
        }

        /// <summary>
        /// Write this Gcmf material definition to the given .GMA stream.
        /// </summary>
        internal void Save(EndianBinaryWriter output, int materialIndex)
        {
            output.Write(Flags);
            output.Write(TextureIdx);
            output.Write(Unk6);
            output.Write(AnisotropyLevel);
            output.Write((uint)0);
            output.Write(UnkC);
            output.Write(Convert.ToUInt16(materialIndex));
            output.Write(Unk10);
            output.Write((uint)0);
            output.Write((uint)0);
            output.Write((uint)0);

        }
    }
}
