using LibGxFormat.ModelLoader;
using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LibGxFormat.Gma
{
    public class GcmfMesh
    {
        internal struct HeaderSectionInfo
        {
            public byte SectionFlags;
            public uint VertexFlags;
            public int Chunk1Size;
            public int Chunk2Size;
        }

        public enum MeshLayer
        {
            /// <summary>
            /// Layer1, for opaque objects.
            /// </summary>
            Layer1,
            /// <summary>
            /// Layer2, for translucid objects.
            /// </summary>
            Layer2
        }

        [Flags]
        public enum RenderFlag
        {
            UnkFlag01 = 0x01,
            /// <summary>
            /// All faces on this mesh are two-sided. Otherwise, faces are one-sided (only the front-facing side is shown).
            /// 
            /// To see the effects of this flag, load init/card.gma, and observe the object MAGCARD.
            /// This object has two meshes, neither or which has the two sided flag set.
            /// The two meshes are completely overlapping one-sided faces, faced opositely.
            /// If the two-sided flag was set on those meshes, there would be a lot of Z-fighting.
            /// 
            /// On the other hand, objects such as chara/arrow_500.gma have this flag set to their cloak meshes
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

        public MeshLayer Layer { get; set; }

        public RenderFlag RenderFlags { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk8 { get; set; }
        public uint UnkC { get; set; }
        public ushort Unk10 { get; set; }
        public ushort Unk14 { get; set; }
        public ushort PrimaryMaterialIdx { get; set; }
        public ushort SecondaryMaterialIdx { get; set; }
        public ushort TertiaryMaterialIdx { get; set; }
        public byte[] TransformMatrixSpecificIdxsObj1 { get; set; }
        public Vector3 BoundingSphereCenter { get; set; }
        public float Unk3C { get; set; }
        public uint Unk40 { get; set; }

        private byte _calculatedUsedMaterialCount;
        public byte calculatedUsedMaterialCount {
            get
            {
                return Convert.ToByte(((PrimaryMaterialIdx != ushort.MaxValue) ? 1 : 0) +
                        ((SecondaryMaterialIdx != ushort.MaxValue) ? 1 : 0) +
                        ((TertiaryMaterialIdx != ushort.MaxValue) ? 1 : 0));
            }
            set
            {
                _calculatedUsedMaterialCount = value;
            }
          
            }

    public GcmfTriangleStripGroup Obj1StripsCcw { get; private set; }
        public GcmfTriangleStripGroup Obj1StripsCw { get; private set; }

        public byte[] TransformMatrixSpecificIdxsObj2 { get; set; }

        public GcmfTriangleStripGroup Obj2StripsCcw { get; private set; }
        public GcmfTriangleStripGroup Obj2StripsCw { get; private set; }

        public GcmfMesh()
        {
            Layer = MeshLayer.Layer1;
            RenderFlags = (RenderFlag)0;
            Unk4 = 0xFFFFFFFF;
            Unk8 = 0x7F7F7FFF;
            UnkC = 0x00000000;
            Unk10 = 0x00FF;
            Unk14 = 0xFF00;
            PrimaryMaterialIdx = ushort.MaxValue;
            SecondaryMaterialIdx = ushort.MaxValue;
            TertiaryMaterialIdx = ushort.MaxValue;

            TransformMatrixSpecificIdxsObj1 = new byte[8];
            for (int i = 0; i < TransformMatrixSpecificIdxsObj1.Length; i++)
                TransformMatrixSpecificIdxsObj1[i] = byte.MaxValue;

            BoundingSphereCenter = new Vector3(0.0f, 0.0f, 0.0f);
            Unk3C = 0;
            Unk40 = 0x00000014;

            Obj1StripsCcw = new GcmfTriangleStripGroup();
            Obj1StripsCw = new GcmfTriangleStripGroup();

            TransformMatrixSpecificIdxsObj2 = new byte[8];
            for (int i = 0; i < TransformMatrixSpecificIdxsObj2.Length; i++)
                TransformMatrixSpecificIdxsObj2[i] = byte.MaxValue;
            Obj2StripsCcw = new GcmfTriangleStripGroup();
            Obj2StripsCw = new GcmfTriangleStripGroup();
        }

        public GcmfMesh(ObjMtlMesh mesh, Dictionary<ObjMtlMaterial, int> modelMaterialMapping)
            : this()
        {
            PrimaryMaterialIdx = Convert.ToUInt16(modelMaterialMapping[mesh.Material]);
            Obj1StripsCcw = new GcmfTriangleStripGroup(mesh);

            RecalculateBoundingSphere();
        }

        /// <summary>
        /// This is the same as render(), but it skips the material setup step,
        /// so when rendering all triangle meshes, they only need to be set up once.
        /// </summary>
        internal void RenderInternal(IRenderer renderer, GcmfRenderContext context)
        {
            /*
            if (Layer == GcmfTriangleMeshLayer.Layer1)
            {
                GL.Disable(EnableCap.Blend);
                GL.DepthMask(true);
            }
            else
            {
                GL.DepthMask(false);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            */

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
            Obj1StripsCcw.Render(renderer, context);

            renderer.SetFrontFaceDirection(FrontFaceDirection.Cw);
            Obj1StripsCw.Render(renderer, context);

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
            Obj2StripsCcw.Render(renderer, context);

            renderer.SetFrontFaceDirection(FrontFaceDirection.Cw);
            Obj2StripsCw.Render(renderer, context);
        }

        internal HeaderSectionInfo LoadHeader(EndianBinaryReader input, MeshLayer newLayer)
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
            byte usedMaterialCount = input.ReadByte();
            byte sectionFlags = input.ReadByte();
            Unk14 = input.ReadUInt16();
            PrimaryMaterialIdx = input.ReadUInt16();
            SecondaryMaterialIdx = input.ReadUInt16();
            TertiaryMaterialIdx = input.ReadUInt16();
            int calculatedUsedMaterialCount = (byte)(
                ((PrimaryMaterialIdx != ushort.MaxValue) ? 1 : 0) +
                ((SecondaryMaterialIdx != ushort.MaxValue) ? 1 : 0) +
                ((TertiaryMaterialIdx != ushort.MaxValue) ? 1 : 0));
            if (calculatedUsedMaterialCount != usedMaterialCount)
                throw new InvalidOperationException("Expected GcmfTriangleMesh[0x13] to match used material count.");
            uint vertexFlags = input.ReadUInt32();
            input.Read(TransformMatrixSpecificIdxsObj1, 0, 8);
            int chunk1Size = input.ReadInt32();
            int chunk2Size = input.ReadInt32();
            BoundingSphereCenter = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
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
                VertexFlags = vertexFlags,
                Chunk1Size = chunk1Size,
                Chunk2Size = chunk2Size
            };
        }

        internal int SizeOfHeader()
        {
            return 0x60;
        }

        internal void SaveHeader(EndianBinaryWriter output, bool isIndexed, bool is16Bit)
        {
            byte sectionFlags = (byte)(
                ((Obj1StripsCcw.Count != 0) ? 0x01 : 0) |
                ((Obj1StripsCw.Count != 0) ? 0x02 : 0) |
                ((Obj2StripsCcw.Count != 0) ? 0x04 : 0) |
                ((Obj2StripsCw.Count != 0) ? 0x08 : 0));

            IEnumerable<GcmfVertex> allVertices = Obj1StripsCcw.Union(Obj1StripsCw)
                .Union(Obj2StripsCcw).Union(Obj2StripsCw).SelectMany(sg => sg);
            uint vertexFlagsCalc = allVertices.First().VertexFlags;
            if (allVertices.All(v => v.VertexFlags != vertexFlagsCalc))
                throw new InvalidGmaFileException("All vertices within the same mesh must have the same components.");

            output.Write((uint)RenderFlags);
            output.Write(Unk4);
            output.Write(Unk8);
            output.Write(UnkC);
            output.Write(Unk10);
            output.Write(calculatedUsedMaterialCount);
            output.Write(sectionFlags);
            output.Write(Unk14);
            output.Write(PrimaryMaterialIdx);
            output.Write(SecondaryMaterialIdx);
            output.Write(TertiaryMaterialIdx);
            output.Write(vertexFlagsCalc);
            output.Write(TransformMatrixSpecificIdxsObj1, 0, 8);
            if (!isIndexed)
            {
                output.Write((Obj1StripsCcw.Count != 0) ? Obj1StripsCcw.SizeOfNonIndexed(is16Bit) : 0);
                output.Write((Obj1StripsCw.Count != 0) ? Obj1StripsCw.SizeOfNonIndexed(is16Bit) : 0);
            }
            else
            {
                // This field contains the number of 32-bit integers instead of the number of bytes, hence the div. by 4
                output.Write((Obj1StripsCcw.Count != 0) ? (Obj1StripsCcw.SizeOfIndexed() / 4) : 0);
                output.Write((Obj1StripsCw.Count != 0) ? (Obj1StripsCw.SizeOfIndexed() / 4) : 0);
            }
            output.Write(BoundingSphereCenter.X);
            output.Write(BoundingSphereCenter.Y);
            output.Write(BoundingSphereCenter.Z);
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

        internal void LoadNonIndexedData(EndianBinaryReader input, HeaderSectionInfo headerSectionInfo, bool is16Bit)
        {
            if ((headerSectionInfo.SectionFlags & 0x01) != 0 && headerSectionInfo.Chunk1Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType1: Chunk1, but chunk1Size == 0?");
            if ((headerSectionInfo.SectionFlags & 0x01) == 0 && headerSectionInfo.Chunk1Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType1: No chunk1, but chunk1Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x01) != 0)
                Obj1StripsCcw.LoadNonIndexed(input, headerSectionInfo.Chunk1Size, headerSectionInfo.VertexFlags, is16Bit);

            if ((headerSectionInfo.SectionFlags & 0x02) != 0 && headerSectionInfo.Chunk2Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType1: Chunk2, but chunk2Size == 0?");
            else if ((headerSectionInfo.SectionFlags & 0x02) == 0 && headerSectionInfo.Chunk2Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType1: No chunk2, but chunk2Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x02) != 0)
                Obj1StripsCw.LoadNonIndexed(input, headerSectionInfo.Chunk2Size, headerSectionInfo.VertexFlags, is16Bit);

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

                Obj2StripsCcw.LoadNonIndexed(input, chunk3Size, headerSectionInfo.VertexFlags, is16Bit);
                Obj2StripsCw.LoadNonIndexed(input, chunk4Size, headerSectionInfo.VertexFlags, is16Bit);
            }
            else if ((headerSectionInfo.SectionFlags & 0xFC) != 0)
            {
                throw new InvalidGmaFileException("GcmfMeshType1: Unknown present chunk flags at chunk10.");
            }
        }

        internal int SizeOfNonIndexedData(bool is16Bit)
        {
            int size = 0;

            if (Obj1StripsCcw.Count != 0)
            {
                size += Obj1StripsCcw.SizeOfNonIndexed(is16Bit);
            }

            if (Obj1StripsCw.Count != 0)
            {
                size += Obj1StripsCw.SizeOfNonIndexed(is16Bit);
            }

            if (Obj2StripsCcw.Count != 0 || Obj2StripsCw.Count != 0)
            {        
                size += 0x20; // Extra header
                size += Obj2StripsCcw.SizeOfNonIndexed(is16Bit);
                size += Obj2StripsCw.SizeOfNonIndexed(is16Bit);
            }

            return size;
        }

        internal void SaveNonIndexedData(EndianBinaryWriter output, bool is16Bit)
        {
            if (Obj1StripsCcw.Count != 0)
            {
                Obj1StripsCcw.SaveNonIndexed(output, is16Bit);
            }

            if (Obj1StripsCw.Count != 0)
            {
                Obj1StripsCw.SaveNonIndexed(output, is16Bit);
            }

            if (Obj2StripsCcw.Count != 0 || Obj2StripsCw.Count != 0)
            {
                // Extra header
                output.Write(TransformMatrixSpecificIdxsObj2, 0, 8);
                output.Write(Obj2StripsCcw.SizeOfNonIndexed(is16Bit));
                output.Write(Obj2StripsCw.SizeOfNonIndexed(is16Bit));
                output.Write(0);
                output.Write(0);
                output.Write(0);
                output.Write(0);

                Obj2StripsCcw.SaveNonIndexed(output, is16Bit);
                Obj2StripsCw.SaveNonIndexed(output, is16Bit);
            }
        }

        internal void LoadIndexedData(EndianBinaryReader input, OrderedSet<GcmfVertex> vertexPool, HeaderSectionInfo headerSectionInfo)
        {
            if ((headerSectionInfo.SectionFlags & 0x01) != 0 && headerSectionInfo.Chunk1Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType2: Chunk1, but chunk1Size == 0?");
            if ((headerSectionInfo.SectionFlags & 0x01) == 0 && headerSectionInfo.Chunk1Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType2: No chunk1, but chunk1Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x01) != 0)
                Obj1StripsCcw.LoadIndexed(input, headerSectionInfo.Chunk1Size, vertexPool, headerSectionInfo.VertexFlags);

            if ((headerSectionInfo.SectionFlags & 0x02) != 0 && headerSectionInfo.Chunk2Size == 0)
                throw new InvalidGmaFileException("GcmfMeshType2: Chunk2, but chunk2Size == 0?");
            else if ((headerSectionInfo.SectionFlags & 0x02) == 0 && headerSectionInfo.Chunk2Size != 0)
                throw new InvalidGmaFileException("GcmfMeshType2: No chunk2, but chunk2Size != 0?");

            if ((headerSectionInfo.SectionFlags & 0x02) != 0)
                Obj1StripsCw.LoadIndexed(input, headerSectionInfo.Chunk2Size, vertexPool, headerSectionInfo.VertexFlags);

            if ((headerSectionInfo.SectionFlags & 0xFC) != 0)
                throw new InvalidGmaFileException("GcmfMeshType2: Unknown present chunk flags at chunk10.");
        }

        internal int SizeOfIndexedData()
        {
            int size = 0;

            if (Obj1StripsCcw.Count != 0)
            {
                size += Obj1StripsCcw.SizeOfIndexed();
            }

            if (Obj1StripsCw.Count != 0)
            {
                size += Obj1StripsCw.SizeOfIndexed();
            }

            return size;
        }

        internal void SaveIndexedData(EndianBinaryWriter output, Dictionary<GcmfVertex, int> vertexPool)
        {
            if (Obj1StripsCcw.Count != 0)
            {
                Obj1StripsCcw.SaveIndexed(output, vertexPool);
            }

            if (Obj1StripsCw.Count != 0)
            {
                Obj1StripsCw.SaveIndexed(output, vertexPool);
            }
        }

        private void RecalculateBoundingSphere()
        {
            IEnumerable<GcmfTriangleStrip> allTriangleStrip = 
                Obj1StripsCcw.Union(Obj1StripsCw).Union(Obj2StripsCcw).Union(Obj2StripsCw);
            IEnumerable<GcmfVertex> allVertices = allTriangleStrip.SelectMany(ts => ts);
            IEnumerable<Vector3> allVertexPositions = allVertices.Select(v => v.Position);

            BoundingSphere boundingSphere = BoundingSphere.FromPoints(allVertexPositions);
            BoundingSphereCenter = boundingSphere.Center;
        }
    };
}
