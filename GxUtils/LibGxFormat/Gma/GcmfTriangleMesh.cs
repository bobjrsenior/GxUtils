using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace LibGxFormat.Gma
{
    public class GcmfTriangleMesh
    {
        internal struct HeaderSectionInfo
        {
            public byte SectionFlags;
            public int Chunk1Size;
            public int Chunk2Size;
        }

        [Flags]
        public enum RenderFlag
        {
            UnkFlag01 = 0x01,
            /// <summary>
            /// All faces on this mesh are two-sided. Otherwise, faces are one-sided (only the front-facing side is shown).
            /// 
            /// To see the effects of this flag, load init/card.gma, and observe the model MAGCARD.
            /// This model has two meshes, neither or which has the two sided flag set.
            /// The two meshes are completely overlapping one-sided faces, faced opositely.
            /// If the two-sided flag was set on those meshes, there would be a lot of Z-fighting.
            /// 
            /// On the other hand, models such as chara/arrow_500.gma have this flag set to their cloak meshes
            /// which is what makes it to be shown on both sides.
            /// </summary>
            TwoSided = 0x02,
            UnkFlag04 = 0x04,
            UnkFlag08 = 0x08,
            UnkFlag10 = 0x10,
            UnkFlag20 = 0x20,
            UnkFlag40 = 0x40,
            UnkFlag200 = 0x200,
        }

        public GcmfTriangleMeshLayer Layer { get; private set; }

        public RenderFlag RenderFlags { get; private set; }
        public uint Unk4 { get; private set; }
        public uint Unk8 { get; private set; }
        public uint UnkC { get; private set; }
        public ushort Unk10 { get; private set; }
        byte usedMaterialCount;
        public ushort Unk14 { get; private set; }
        public ushort PrimaryMaterialIdx { get; private set; }
        public ushort SecondaryMaterialIdx { get; private set; }
        public ushort TertiaryMaterialIdx { get; private set; }
        uint vertexFlags; // See GcmfVertexFlags; TODO recalculate on save
        public byte[] TransformMatrixSpecificIdxsObj1 { get; private set; }
        public Vector3 Center { get; private set; }
        public float Unk3C { get; private set; }
        public uint Unk40 { get; private set; }

        GcmfTriangleStripGroup obj1StripsCcw;
        GcmfTriangleStripGroup obj1StripsCw;

        public byte[] TransformMatrixSpecificIdxsObj2 { get; private set; }

        GcmfTriangleStripGroup obj2StripsCcw;
        GcmfTriangleStripGroup obj2StripsCw;

        public GcmfTriangleMesh()
        {
            TransformMatrixSpecificIdxsObj1 = new byte[8];
            for (int i = 0; i < TransformMatrixSpecificIdxsObj1.Length; i++)
                TransformMatrixSpecificIdxsObj1[i] = byte.MaxValue;
            obj1StripsCcw = new GcmfTriangleStripGroup();
            obj1StripsCw = new GcmfTriangleStripGroup();

            TransformMatrixSpecificIdxsObj2 = new byte[8];
            for (int i = 0; i < TransformMatrixSpecificIdxsObj2.Length; i++)
                TransformMatrixSpecificIdxsObj2[i] = byte.MaxValue;
            obj2StripsCcw = new GcmfTriangleStripGroup();
            obj2StripsCw = new GcmfTriangleStripGroup();
        }

        /// <summary>
        /// This is the same as render(), but it skips the material setup step,
        /// so when rendering all triangle meshes, they only need to be set up once.
        /// </summary>
        internal void RenderInternal(IRenderer renderer, GcmfRenderContext context)
        {
            // Set up the renderer according to the render flags set
            renderer.SetTwoSidedFaces((RenderFlags & RenderFlag.TwoSided) != 0);

            // Very rarely a mesh with a material id set to 0xFFFF ((ushort)-1) is seen.
            // I believe this is just a that it didn't have a material assigned.
            if (PrimaryMaterialIdx != ushort.MaxValue)
            {
                renderer.BindMaterial(PrimaryMaterialIdx);
            }
            else
            {
                renderer.UnbindMaterial();
            }

            // Patch the non-null default transformation matrix indexes (for obj1)
            // with the specific indexes set for this mesh
            for (int i = 0; i < TransformMatrixSpecificIdxsObj1.Length; i++)
            {
                if (TransformMatrixSpecificIdxsObj1[i] != byte.MaxValue)
                {
                    context.TransformMatrixIdxs[i] = TransformMatrixSpecificIdxsObj1[i];
                }
            }

            renderer.SetFrontFaceDirection(FrontFaceDirection.Ccw);
            obj1StripsCcw.Render(renderer, context);

            renderer.SetFrontFaceDirection(FrontFaceDirection.Cw);
            obj1StripsCw.Render(renderer, context);

            // Patch the non-null default transformation matrix indexes (for obj2)
            // with the specific indexes set for this mesh
            for (int i = 0; i < TransformMatrixSpecificIdxsObj2.Length; i++)
            {
                if (TransformMatrixSpecificIdxsObj2[i] != byte.MaxValue)
                {
                    context.TransformMatrixIdxs[i] = TransformMatrixSpecificIdxsObj2[i];
                }
            }

            renderer.SetFrontFaceDirection(FrontFaceDirection.Ccw);
            obj2StripsCcw.Render(renderer, context);

            renderer.SetFrontFaceDirection(FrontFaceDirection.Cw);
            obj2StripsCw.Render(renderer, context);
        }

        internal HeaderSectionInfo LoadHeader(EndianBinaryReader input, GcmfTriangleMeshLayer newLayer)
        {
            Layer = newLayer;

            uint renderFlagsUint = input.ReadUInt32();
            if ((renderFlagsUint & ~(uint)(RenderFlag.UnkFlag01 |
                                           RenderFlag.TwoSided |
                                           RenderFlag.UnkFlag04 |
                                           RenderFlag.UnkFlag08 |
                                           RenderFlag.UnkFlag10 |
                                           RenderFlag.UnkFlag20 |
                                           RenderFlag.UnkFlag40 |
                                           RenderFlag.UnkFlag200)) != 0)
            {
                throw new InvalidOperationException("Unknown RenderFlags set.");
            }
            RenderFlags = (RenderFlag)renderFlagsUint;

            Unk4 = input.ReadUInt32();
            Unk8 = input.ReadUInt32();
            UnkC = input.ReadUInt32();
            Unk10 = input.ReadUInt16();
            usedMaterialCount = input.ReadByte();
            byte sectionFlags = input.ReadByte();
            Unk14 = input.ReadUInt16();
            PrimaryMaterialIdx = input.ReadUInt16();
            SecondaryMaterialIdx = input.ReadUInt16();
            TertiaryMaterialIdx = input.ReadUInt16();
            vertexFlags = input.ReadUInt32();
            input.Read(TransformMatrixSpecificIdxsObj1, 0, 8);
            int chunk1Size = input.ReadInt32();
            int chunk2Size = input.ReadInt32();
            Center = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
            Unk3C = input.ReadSingle();
            Unk40 = input.ReadUInt32();
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x44] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x48] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x4C] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x50] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x54] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x58] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected GcmfTriangleMesh[0x5C] == 0");

            return new HeaderSectionInfo
            {
                SectionFlags = sectionFlags,
                Chunk1Size = chunk1Size,
                Chunk2Size = chunk2Size
            };
        }

        internal int SizeOfHeader()
        {
            return 0x60;
        }

        internal void SaveHeader(EndianBinaryWriter output, bool isIndexed)
        {
            byte sectionFlags = (byte)(
                (!obj1StripsCcw.IsEmpty ? 0x01 : 0) |
                (!obj1StripsCw.IsEmpty ? 0x02 : 0) |
                (!obj2StripsCcw.IsEmpty ? 0x04 : 0) |
                (!obj2StripsCw.IsEmpty ? 0x08 : 0));

            output.Write((uint)RenderFlags);
            output.Write(Unk4);
            output.Write(Unk8);
            output.Write(UnkC);
            output.Write(Unk10);
            output.Write(usedMaterialCount);
            output.Write(sectionFlags);
            output.Write(Unk14);
            output.Write(PrimaryMaterialIdx);
            output.Write(SecondaryMaterialIdx);
            output.Write(TertiaryMaterialIdx);
            output.Write(vertexFlags);
            output.Write(TransformMatrixSpecificIdxsObj1, 0, 8);
            if (!isIndexed)
            {
                output.Write(!obj1StripsCcw.IsEmpty ? obj1StripsCcw.SizeOfNonIndexed() : 0);
                output.Write(!obj1StripsCw.IsEmpty ? obj1StripsCw.SizeOfNonIndexed() : 0);
            }
            else
            {
                // This field contains the number of 32-bit integers instead of the number of bytes, hence the div. by 4
                output.Write(!obj1StripsCcw.IsEmpty ? (obj1StripsCcw.SizeOfIndexed() / 4) : 0);
                output.Write(!obj1StripsCw.IsEmpty ? (obj1StripsCw.SizeOfIndexed() / 4) : 0);
            }
            output.Write(Center.X);
            output.Write(Center.Y);
            output.Write(Center.Z);
            output.Write(Unk3C);
            output.Write(Unk40);
            output.Write(0);
            output.Write(0);
            output.Write(0);
            output.Write(0);
            output.Write(0);
            output.Write(0);
            output.Write(0);
        }

        internal void LoadNonIndexedData(EndianBinaryReader input, HeaderSectionInfo headerSectionInfo)
        {
            if ((headerSectionInfo.SectionFlags & 0x01) != 0 && headerSectionInfo.Chunk1Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType1: Chunk1, but chunk1Size == 0?");
            if ((headerSectionInfo.SectionFlags & 0x01) == 0 && headerSectionInfo.Chunk1Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType1: No chunk1, but chunk1Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x01) != 0)
                obj1StripsCcw.LoadNonIndexed(input, headerSectionInfo.Chunk1Size, vertexFlags);

            if ((headerSectionInfo.SectionFlags & 0x02) != 0 && headerSectionInfo.Chunk2Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType1: Chunk2, but chunk2Size == 0?");
            else if ((headerSectionInfo.SectionFlags & 0x02) == 0 && headerSectionInfo.Chunk2Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType1: No chunk2, but chunk2Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x02) != 0)
                obj1StripsCw.LoadNonIndexed(input, headerSectionInfo.Chunk2Size, vertexFlags);

            if ((headerSectionInfo.SectionFlags & 0xFF) == 0x0F) // Those are always used together
            {
                // Read extra header before two extra chunks
                input.Read(TransformMatrixSpecificIdxsObj2, 0, 8);
                int chunk3Size = input.ReadInt32();
                int chunk4Size = input.ReadInt32();
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Expected GcmfMeshType1[ExtraHdr-0x10] == 0");
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Expected GcmfMeshType1[ExtraHdr-0x14] == 0");
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Expected GcmfMeshType1[ExtraHdr-0x18]== 0");
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Expected GcmfMeshType1[ExtraHdr-0x1C] == 0");

                if (chunk3Size == 0)
                    throw new InvalidGmaFileException("GcmfMeshType1: Chunk3, but chunk3Size == 0?");

                if (chunk4Size == 0)
                    throw new InvalidGmaFileException("GcmfMeshType1: Chunk4, but chunk4Size == 0?");

                obj2StripsCcw.LoadNonIndexed(input, chunk3Size, vertexFlags);
                obj2StripsCw.LoadNonIndexed(input, chunk4Size, vertexFlags);
            }
            else if ((headerSectionInfo.SectionFlags & 0xFC) != 0)
            {
                throw new InvalidGmaFileException("GcmfMeshType1: Unknown present chunk flags at chunk10.");
            }
        }

        internal int SizeOfNonIndexedData()
        {
            int size = 0;

            if (!obj1StripsCcw.IsEmpty)
            {
                size += obj1StripsCcw.SizeOfNonIndexed();
            }

            if (!obj1StripsCw.IsEmpty)
            {
                size += obj1StripsCw.SizeOfNonIndexed();
            }

            if (!obj2StripsCcw.IsEmpty || !obj2StripsCw.IsEmpty)
            {        
                size += 0x20; // Extra header
                size += obj2StripsCcw.SizeOfNonIndexed();
                size += obj2StripsCw.SizeOfNonIndexed();
            }

            return size;
        }

        internal void SaveNonIndexedData(EndianBinaryWriter output)
        {
            if (!obj1StripsCcw.IsEmpty)
            {
                obj1StripsCcw.SaveNonIndexed(output);
            }

            if (!obj1StripsCw.IsEmpty)
            {
                obj1StripsCw.SaveNonIndexed(output);
            }

            if (!obj2StripsCcw.IsEmpty || !obj2StripsCw.IsEmpty)
            {
                // Extra header
                output.Write(TransformMatrixSpecificIdxsObj2, 0, 8);
                output.Write(obj2StripsCcw.SizeOfNonIndexed());
                output.Write(obj2StripsCw.SizeOfNonIndexed());
                output.Write(0);
                output.Write(0);
                output.Write(0);
                output.Write(0);

                obj2StripsCcw.SaveNonIndexed(output);
                obj2StripsCw.SaveNonIndexed(output);
            }
        }

        internal void LoadIndexedData(EndianBinaryReader input, IList<GcmfVertex> vertexPool, HeaderSectionInfo headerSectionInfo)
        {
            if ((headerSectionInfo.SectionFlags & 0x01) != 0 && headerSectionInfo.Chunk1Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType2: Chunk1, but chunk1Size == 0?");
            if ((headerSectionInfo.SectionFlags & 0x01) == 0 && headerSectionInfo.Chunk1Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType2: No chunk1, but chunk1Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x01) != 0)
                obj1StripsCcw.LoadIndexed(input, headerSectionInfo.Chunk1Size, vertexPool, vertexFlags);

            if ((headerSectionInfo.SectionFlags & 0x02) != 0 && headerSectionInfo.Chunk2Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType2: Chunk2, but chunk2Size == 0?");
            else if ((headerSectionInfo.SectionFlags & 0x02) == 0 && headerSectionInfo.Chunk2Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType2: No chunk2, but chunk2Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x02) != 0)
                obj1StripsCw.LoadIndexed(input, headerSectionInfo.Chunk2Size, vertexPool, vertexFlags);

            if ((headerSectionInfo.SectionFlags & 0xFC) != 0)
                throw new InvalidGmaFileException("GcmfMeshType2: Unknown present chunk flags at chunk10.");
        }

        internal int SizeOfIndexedData()
        {
            int size = 0;

            if (!obj1StripsCcw.IsEmpty)
            {
                size += obj1StripsCcw.SizeOfIndexed();
            }

            if (!obj1StripsCw.IsEmpty)
            {
                size += obj1StripsCw.SizeOfIndexed();
            }

            return size;
        }

        internal void SaveIndexedData(EndianBinaryWriter output)
        {
            if (!obj1StripsCcw.IsEmpty)
            {
                obj1StripsCcw.SaveIndexed(output);
            }

            if (!obj1StripsCw.IsEmpty)
            {
                obj1StripsCw.SaveIndexed(output);
            }
        }
    };
}
