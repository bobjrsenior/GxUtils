using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using LibGxFormat.ModelLoader;
using System.Drawing;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// A Gcmf object contains the definition for a single object in a model.
    /// </summary>
    public class Gcmf : IRenderable
    {
        [Flags]
        enum GcmfSectionFlags : uint
        {
            /// <summary>
            /// 16 bit flag (vertices are stored as uint16 format instead of float)
            /// </summary>
            _16Bit = 0x01,
            /// <summary>
            /// Called "Stitching Model" in the debug menu. Has associated transform matrices.
            /// </summary>
            StitchingModel = 0x04,
            /// <summary>
            /// Called "Skin Model" in the debug menu. Has associated transform matrices and indexed vartices.
            /// </summary>
            SkinModel = 0x08,
            /// <summary>
            /// Called "Effective Model" in the debug menu. Has indexed vertices.
            /// </summary>
            EffectiveModel = 0x10,
        }

        /// <summary>
        /// Magic number in the GCMF header. Corresponds to ASCII "GCMF"
        /// </summary>
        private const uint GcmfMagic = 0x47434D46;

        public uint SectionFlags { get; private set; }

        /// <summary>
        /// Position of the center of the object (used along with the radius below)
        /// </summary>
        public Vector3 BoundingSphereCenter { get; private set; }

        /// <summary>
        /// Radius of the object (maximum distance to a vertex relative to the center)
        /// </summary>
        public float BoundingSphereRadius { get; private set; }

        public byte[] TransformMatrixDefaultIdxs { get; private set; }

        /// <summary>
        /// List of materials defined for this object.
        /// </summary>
        public NonNullableCollection<GcmfMaterial> Materials { get; private set; }

        // TODO limit access when no transform type
        public NonNullableCollection<GcmfTransformMatrix> TransformMatrices { get; private set; }

        /// <summary>
        /// Vertex pool for indexed objects.
        /// </summary>
        public OrderedSet<GcmfVertex> VertexPool { get; private set; }

        public NonNullableCollection<GcmfMesh> Meshes { get; private set; }

        // TODO limit access when not type=8
        public NonNullableCollection<GcmfType8Unknown1> Type8Unknown1 { get; private set; }

        // TODO limit access when not type=8
        public Collection<ushort> Type8Unknown2 { get; private set; }

        /// <summary>
        /// Returns true if the vertices in this GCMF file are stored in indexed format, false otherwise.
        /// </summary>
        public bool IsIndexed
        {
            get
            {
                return (
                    (SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0 ||
                    (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0);
            }
        }

        public Gcmf()
        {
            SectionFlags = 0;
            BoundingSphereCenter = new Vector3(0.0f, 0.0f, 0.0f);
            BoundingSphereRadius = 0.0f;
            TransformMatrixDefaultIdxs = new byte[8];
            for (int i = 0; i < TransformMatrixDefaultIdxs.Length; i++)
                TransformMatrixDefaultIdxs[i] = byte.MaxValue;
            Materials = new NonNullableCollection<GcmfMaterial>();
            TransformMatrices = new NonNullableCollection<GcmfTransformMatrix>();
            VertexPool = new OrderedSet<GcmfVertex>();
            Meshes = new NonNullableCollection<GcmfMesh>();
            Type8Unknown1 = new NonNullableCollection<GcmfType8Unknown1>();
            Type8Unknown2 = new Collection<ushort>();
        }

        /// <summary>
        /// Create a new Gcmf object from the given .OBJ / .MTL object.
        /// </summary>
        /// <param name="modelObject">The object from which to create the Gcmf object.</param>
        /// <param name="textureIndexMapping">Correspondence between the textures defined in the model materials and .TPL texture indices.</param>
        public Gcmf(ObjMtlObject modelObject, Dictionary<Bitmap, int> modelTextureMapping)
            : this()
        {
            Dictionary<ObjMtlMaterial, int> modelMaterialMapping = new Dictionary<ObjMtlMaterial, int>();

            foreach (ObjMtlMaterial mat in modelObject.Meshes.Select(m => m.Material))
            {
                modelMaterialMapping.Add(mat, Materials.Count);
                Materials.Add(new GcmfMaterial(mat, modelTextureMapping));
            }

            foreach (ObjMtlMesh mesh in modelObject.Meshes)
            {
                Meshes.Add(new GcmfMesh(mesh, modelMaterialMapping));
            }

            UpdateBoundingSphere();
        }

        internal void Load(EndianBinaryReader input, GxGame game)
        {
            int baseOffset = Convert.ToInt32(input.BaseStream.Position);

            // Load GCMF header
            if (input.ReadUInt32() != GcmfMagic)
                throw new InvalidGmaFileException("Expected Gcmf[0x00] == GcmfMagic.");
            SectionFlags = input.ReadUInt32();
            BoundingSphereCenter = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
            BoundingSphereRadius = input.ReadSingle();
            int numMaterials = (int)input.ReadUInt16();
            int numLayer1Meshes = (int)input.ReadUInt16();
            int numLayer2Meshes = (int)input.ReadUInt16();
            int transformMatrixCount = (int)input.ReadByte();
            if (input.ReadByte() != 0)
                throw new InvalidGmaFileException("Expected Gcmf[0x1F] == 0");
            int headerSize = input.ReadInt32();

            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected Gcmf[0x24] == 0");
            input.Read(TransformMatrixDefaultIdxs, 0, 8);
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected Gcmf[0x30] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected Gcmf[0x34] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected Gcmf[0x38] == 0");
            if (input.ReadUInt32() != 0)
                throw new InvalidGmaFileException("Expected Gcmf[0x3C] == 0");

            // Load materials
            for (int i = 0; i < numMaterials; i++)
            {
                GcmfMaterial mat = new GcmfMaterial();
                mat.Load(input, i);
                Materials.Add(mat);
            }

            if ((SectionFlags & (uint)~(GcmfSectionFlags._16Bit |
                                        GcmfSectionFlags.StitchingModel |
                                        GcmfSectionFlags.SkinModel |
                                        GcmfSectionFlags.EffectiveModel)) != 0)
            {
                throw new InvalidGmaFileException("Unknown GCMF section flags.");
            }

            if ((SectionFlags & (uint)GcmfSectionFlags.StitchingModel) != 0 ||
                (SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0)
            {
                for (int i = 0; i < transformMatrixCount; i++)
                {
                    GcmfTransformMatrix tmtx = new GcmfTransformMatrix();
                    tmtx.Load(input);
                    TransformMatrices.Add(tmtx);
                }
            }
            else
            {
                if (transformMatrixCount != 0)
                    throw new InvalidGmaFileException("GcmfSection: No transform matrices expected, but transformMatrixCount != 0?");
            }

            if (PaddingUtils.Align(Convert.ToInt32(input.BaseStream.Position), 0x20) != baseOffset + headerSize)
                throw new InvalidGmaFileException("Gcmf [End Header Offset] mismatch.");

            input.BaseStream.Position = baseOffset + headerSize;

            if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0 ||
                (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0)
            {
                int sectionBaseOffset = Convert.ToInt32(input.BaseStream.Position);

                int numVertices = input.ReadInt32();
                int offsetPartType8Unknown1 = input.ReadInt32();
                int offsetPartVertexPool = input.ReadInt32();
                int offsetPartMeshData = input.ReadInt32();
                int offsetPartType8Unknown2 = input.ReadInt32();
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Gcmf[PreSectionHdr-0x14]");
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Gcmf[PreSectionHdr-0x18]");
                if (input.ReadUInt32() != 0)
                    throw new InvalidGmaFileException("Gcmf[PreSectionHdr-0x1C]");

                // Load the mesh headers
                List<GcmfMesh.HeaderSectionInfo> meshHeaderSectionInfos = new List<GcmfMesh.HeaderSectionInfo>();
                for (int i = 0; i < numLayer1Meshes + numLayer2Meshes; i++)
                {
                    GcmfMesh mesh = new GcmfMesh();
                    meshHeaderSectionInfos.Add(mesh.LoadHeader(input,
                        (i < numLayer1Meshes) ? GcmfMesh.MeshLayer.Layer1 : GcmfMesh.MeshLayer.Layer2));
                    Meshes.Add(mesh);
                }

                if (Convert.ToInt32(input.BaseStream.Position) != sectionBaseOffset + offsetPartVertexPool)
                    throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPartVertexPool doesn't match expected value.");

                // Load the vertex pool, i.e. the vertices referenced from the indexed triangle strips
                for (int i = 0; i < numVertices; i++)
                {
                    GcmfVertex vtx = new GcmfVertex();
                    vtx.LoadIndexed(input);
                    VertexPool.Add(vtx);
                }

                if ((game == GxGame.SuperMonkeyBall || game == GxGame.SuperMonkeyBallDX) && (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0)
                {
                    // SMB doesn't have have any 0x08 section flags, so it's unknown how this field may work in that case
                    if (offsetPartType8Unknown1 != 0)
                        throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPartType8Unknown1 is not zero on SMB.");
                }
                else
                {
                    if (Convert.ToInt32(input.BaseStream.Position) != sectionBaseOffset + offsetPartType8Unknown1)
                        throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPartType8Unknown1 doesn't match expected value.");
                }

                if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0)
                {
                    for (int i = 0; i < (offsetPartType8Unknown2-offsetPartType8Unknown1)/0x20; i++)
                    {
                        GcmfType8Unknown1 unk1 = new GcmfType8Unknown1();
                        unk1.Load(input);
                        Type8Unknown1.Add(unk1);
                    }
                }

                if ((game == GxGame.SuperMonkeyBall || game == GxGame.SuperMonkeyBallDX) && (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0)
                {
                    // SMB doesn't have have any 0x08 section flags, so it's unknown how this field may work in that case
                    if (offsetPartType8Unknown2 != 0)
                        throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPartType8Unknown2 is not zero on SMB.");
                }
                else
                {
                    if (Convert.ToInt32(input.BaseStream.Position) != sectionBaseOffset + offsetPartType8Unknown2)
                        throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPartType8Unknown2 doesn't match expected value.");
                }

                if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0)
                {
                    // TODO figure out a better way to calculate this
                    int numEntriesPart3 = Type8Unknown1.Max(u1 => u1.unk18) + 1;
                    for (int i = 0; i < numEntriesPart3; i++)
                        Type8Unknown2.Add(input.ReadUInt16());

                    if (PaddingUtils.Align(Convert.ToInt32(input.BaseStream.Position), 0x20) != sectionBaseOffset + offsetPartMeshData)
                        throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPart4 doesn't match expected value.");

                    input.BaseStream.Position = sectionBaseOffset + offsetPartMeshData;
                }

                if (Convert.ToInt32(input.BaseStream.Position) != sectionBaseOffset + offsetPartMeshData)
                    throw new InvalidGmaFileException("Gcmf [PreSectionHdr] offsetPartMeshData doesn't match expected value.");

                // Load the mesh data itself (the indexed triangle strips)
                for (int i = 0; i < numLayer1Meshes + numLayer2Meshes; i++)
                {
                    Meshes[i].LoadIndexedData(input, VertexPool, meshHeaderSectionInfos[i]);
                }

                // Make sure that all the vertices in the vertex pool got their
                // vertex flags set when we read the indexed triangle strips,
                // that is, that they were referenced at least once.
                // Otherwise, we would have semi-initialized vertices in the vertex pool!
                if (VertexPool.Any(vtx => !vtx.IsIndexedVertexInitialized()))
                {
                    throw new InvalidGmaFileException("Not all vertex flags of all vertices in the vertex pool got initialized!");
                }
            }
            else
            {
                for (int i = 0; i < numLayer1Meshes + numLayer2Meshes; i++)
                {
                    GcmfMesh mesh = new GcmfMesh();
                    GcmfMesh.HeaderSectionInfo headerSectionInfo = mesh.LoadHeader(input,
                        (i < numLayer1Meshes) ? GcmfMesh.MeshLayer.Layer1 : GcmfMesh.MeshLayer.Layer2);
                    mesh.LoadNonIndexedData(input, headerSectionInfo, (SectionFlags & (uint)GcmfSectionFlags._16Bit) != 0);
                    Meshes.Add(mesh);
                }
            }
        }

        internal int SizeOf()
        {
            return SizeOfHeader() + SizeOfSections();
        }

        private int SizeOfHeader()
        {
            return PaddingUtils.Align(0x40 +
                Materials.Sum(mat => mat.SizeOf()) +
                TransformMatrices.Sum(tmtx => tmtx.SizeOf()), 0x20);
        }

        private int SizeOfSections()
        {
            int size = 0;

            if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0 || (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0)
            {
                size += SizeOf2Header();
                size += SizeOf2MeshHeaders();
                size += SizeOf2VertexPool();

                if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0)
                {
                    size += SizeOf2Type8Unknown1();
                    size += SizeOf2Type8Unknown2();
                }

                size += SizeOf2MeshData();
            }
            else
            {
                bool is16Bit = ((SectionFlags & (uint)GcmfSectionFlags._16Bit) != 0);
                foreach (GcmfMesh mesh in Meshes)
                {
                    size += mesh.SizeOfHeader();
                    size += mesh.SizeOfNonIndexedData(is16Bit);
                }
            }

            return size;
        }

        private int SizeOf2Header()
        {
            return 0x20;
        }

        private int SizeOf2MeshHeaders()
        {
            return Meshes.Sum(mesh => mesh.SizeOfHeader());
        }

        private int SizeOf2VertexPool()
        {
            return VertexPool.Sum(vtx => vtx.SizeOfIndexed());
        }

        private int SizeOf2Type8Unknown1()
        {
            return Type8Unknown1.Sum(unk1 => unk1.SizeOf());
        }

        private int SizeOf2Type8Unknown2()
        {
            return PaddingUtils.Align(2 * Type8Unknown2.Count, 0x20);
        }

        private int SizeOf2MeshData()
        {
            return PaddingUtils.Align(Meshes.Sum(mesh => mesh.SizeOfIndexedData()), 0x20);
        }

        internal void Save(EndianBinaryWriter output, GxGame game)
        {
            int headerSize = SizeOfHeader();

            // In the GCMF file, the triangle meshes are classified in two layers:
            // The "opaque" layer and the "translucid" layer.
            // We store both kinds of layers in the same vector for easier manipulation
            // (setting the layer property on the triangle mesh itself), which the user
            // may order freely, but now that we're writting the GCMF again, we need to
            // reclassify the meshes in both layers.
            GcmfMesh[] layer1Meshes = Meshes.Where(mesh => mesh.Layer == GcmfMesh.MeshLayer.Layer1).ToArray();
            GcmfMesh[] layer2Meshes = Meshes.Where(mesh => mesh.Layer == GcmfMesh.MeshLayer.Layer2).ToArray();
            GcmfMesh[] meshesSortedByLayer = layer1Meshes.Union(layer2Meshes).ToArray();

            // Write GCMF header
            output.Write(GcmfMagic);
            output.Write(SectionFlags);
            output.Write(BoundingSphereCenter.X);
            output.Write(BoundingSphereCenter.Y);
            output.Write(BoundingSphereCenter.Z);
            output.Write(BoundingSphereRadius);
            output.Write(Convert.ToUInt16(Materials.Count));
            output.Write(Convert.ToUInt16(layer1Meshes.Length));
            output.Write(Convert.ToUInt16(layer2Meshes.Length));
            output.Write(Convert.ToByte(TransformMatrices.Count));
            output.Write((byte)0);
            output.Write(headerSize);
            output.Write((uint)0);
            output.Write(TransformMatrixDefaultIdxs);
            output.Write((uint)0);
            output.Write((uint)0);
            output.Write((uint)0);
            output.Write((uint)0);

            for (int i = 0; i < Materials.Count; i++)
                Materials[i].Save(output, i);

            foreach (GcmfTransformMatrix tmtx in TransformMatrices)
                tmtx.Save(output);

            output.Align(0x20);

            if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0 || (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0)
            {
                int offsetPartVertexPool = SizeOf2Header() + SizeOf2MeshHeaders();
                int offsetPartType8Unknown1 = offsetPartVertexPool + SizeOf2VertexPool();
                int offsetPartType8Unknown2 = offsetPartType8Unknown1 + (((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0) ? SizeOf2Type8Unknown1() : 0);
                int offsetPartMeshData = offsetPartType8Unknown2 + (((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0) ? SizeOf2Type8Unknown2() : 0);
                if ((game == GxGame.SuperMonkeyBall || game == GxGame.SuperMonkeyBallDX) && (SectionFlags & (uint)GcmfSectionFlags.EffectiveModel) != 0)
                {
                    // Those are zero on SMB
                    offsetPartType8Unknown1 = 0;
                    offsetPartType8Unknown2 = 0;
                }

                output.Write(VertexPool.Count);
                output.Write(offsetPartType8Unknown1);
                output.Write(offsetPartVertexPool);
                output.Write(offsetPartMeshData);
                output.Write(offsetPartType8Unknown2);
                output.Write((uint)0);
                output.Write((uint)0);
                output.Write((uint)0);

                // Write the mesh headers
                foreach (GcmfMesh mesh in meshesSortedByLayer)
                    mesh.SaveHeader(output, true, false);

                // Write the vertex pool
                foreach (GcmfVertex vtx in VertexPool)
                    vtx.SaveIndexed(output);

                if ((SectionFlags & (uint)GcmfSectionFlags.SkinModel) != 0)
                {
                    foreach (GcmfType8Unknown1 unk1 in Type8Unknown1)
                        unk1.Save(output);

                    foreach (ushort unk2 in Type8Unknown2)
                        output.Write(unk2);

                    output.Align(0x20);
                }

                // Write the section data itself (the indexed triangle strips)
                Dictionary<GcmfVertex, int> vertexPoolIndexes = new Dictionary<GcmfVertex, int>();
                for (int i = 0; i < VertexPool.Count; i++)
                    vertexPoolIndexes.Add(VertexPool[i], i);

                foreach (GcmfMesh mesh in meshesSortedByLayer)
                    mesh.SaveIndexedData(output, vertexPoolIndexes);

                output.Align(0x20);
            }
            else
            {
                bool is16Bit = ((SectionFlags & (uint)GcmfSectionFlags._16Bit) != 0);
                foreach (GcmfMesh mesh in meshesSortedByLayer)
                {
                    mesh.SaveHeader(output, false, is16Bit);
                    mesh.SaveNonIndexedData(output, is16Bit);
                }
            }
        }

        /// <summary>
        /// Render this Gcmf object using the given renderer.
        /// </summary>
        public void Render(IRenderer renderer)
        {
            GcmfRenderContext context = new GcmfRenderContext(this);

            // Set up the default transform matrix indexes in the context
            // Those may get patched later in each triangle mesh
            // Note that those are not re-set for every mesh!
            // The specific indexes set on one mesh may affect the next meshes
            Array.Copy(TransformMatrixDefaultIdxs, context.TransformMatrixIdxs, 8);

            // Set up the materials required to render each mesh
            for (int i = 0; i < Materials.Count; i++)
                Materials[i].Render(renderer, i);

            // Render each mesh
            for (int i = 0; i < Meshes.Count; i++)
            {
                renderer.BeginObject(string.Format("Mesh{0}", i));
                Meshes[i].RenderInternal(renderer, context);
                renderer.EndObject();
            }

            // Clean up the materials in order to have a clean ground for the next object
            renderer.ClearMaterialList();
        }

        private void UpdateBoundingSphere()
        {
            IEnumerable<GcmfTriangleStrip> allTriangleStrip = Meshes.SelectMany(
                m => m.Obj1StripsCcw.Union(m.Obj1StripsCw).Union(m.Obj2StripsCcw).Union(m.Obj2StripsCw));
            IEnumerable<GcmfVertex> allVertices = allTriangleStrip.SelectMany(ts => ts);
            IEnumerable<Vector3> allVertexPositions = allVertices.Select(v => v.Position);

            BoundingSphere boundingSphere = BoundingSphere.FromPoints(allVertexPositions);
            BoundingSphereCenter = boundingSphere.Center;
            BoundingSphereRadius = boundingSphere.Radius;
        }
    }
}
