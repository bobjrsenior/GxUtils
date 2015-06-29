using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK.Graphics.OpenGL;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// A Gcmf material entry contains the definition of a single model material.
    /// </summary>
    public class GcmfMaterial
    {
        // OR of all flags of all GMA files: 0x5AFFFF
        // Bits 0x000000C - Texture Wrap S (0 - Clamp, 1 - Repeat, 2 - Mirror)
        // Bits 0x0000030 - Texture Wrap T (0 - Clamp, 1 - Repeat, 2 - Mirror)
        // Bits 0x0000040 - Anisotropic filtering? (Seems to be VERY related, but sometimes anisotropyLevel is set without this..)
        // Bits 0x0000700 - Related to mipmaping (number/proportion of mipmap levels to use?)
        //                  If set to zero, shows as XXX & MIPMAP NEAR - XXX
        //                  With increasing values, more mipmaps are shown big in the debug menu.
        // Bits 000000800 - Near mag & min filter (Shows as NEAR & MIPMAP XXX - NEAR in the debug menu)
        // Bits 0x0020000 - Makes some textures display a scrolling animation. Used for example for the booster pad animation.
        public uint Flags { get; private set; }
        /// <summary>
        /// Index of the texture in the .TPL file.
        /// </summary>
        public ushort TextureIdx { get; private set; }
        public byte Unk6 { get; private set; }
        public byte AnisotropyLevel { get; private set; } // ANISO level (0 = ANISO 1, 1 = ANISO 2, 2 = ANISO 4)
        public ushort UnkC { get; private set; }
        public ushort Index { get; private set; }
        public uint Unk10 { get; private set; }

        /// <summary>
        /// Set up this Gcmf material definition in the given material renderer.
        /// </summary>
        internal void Render(IRenderer renderer)
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

            renderer.DefineMaterial(Index, TextureIdx, wrapS, wrapT);
        }

        /// <summary>
        /// Load a Gcmf material definition from the given .GMA stream.
        /// </summary>
        internal void Load(EndianBinaryReader input)
        {
            Flags = input.ReadUInt32();
            TextureIdx = input.ReadUInt16();
            Unk6 = input.ReadByte();
            AnisotropyLevel = input.ReadByte();
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfMaterial[0x08] == 0");
            UnkC = input.ReadUInt16();
            Index = input.ReadUInt16();
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
        internal void Save(EndianBinaryWriter output)
        {
            output.Write(Flags);
            output.Write(TextureIdx);
            output.Write(Unk6);
            output.Write(AnisotropyLevel);
            output.Write((uint)0);
            output.Write(UnkC);
            output.Write(Index);
            output.Write(Unk10);
            output.Write((uint)0);
            output.Write((uint)0);
            output.Write((uint)0);

        }
    }
}
