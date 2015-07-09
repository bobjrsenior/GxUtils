using LibGxFormat.ModelLoader;
using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibGxFormat.Gma
{
    public class GcmfTriangleStrip : NonNullableCollection<GcmfVertex>
    {
        enum GcmfNonIndexedVertexDataType : byte
        {
            Float = 0x98,
            Uint16 = 0x99
        }

        /// <summary>
        /// Create a new empty Gcmf triangle strip.
        /// </summary>
        public GcmfTriangleStrip()
        {
        }

        public GcmfTriangleStrip(GcmfTriangleStripGroup parentStripGroup, IEnumerable<ObjMtlVertex> vertexList)
        {
            foreach (ObjMtlVertex vtx in vertexList)
                Add(new GcmfVertex(vtx));
        }
        
        internal void Render(IRenderer renderer, GcmfRenderContext context)
        {
            // Convert GcmfVertex list to ModelVertex list
            ModelVertex[] modelVertices = new ModelVertex[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                GcmfVertex gcmfVtx = Items[i];

                // Copy the data from the GCMF vertex to the ModelVertex
                ModelVertex modelVtx = new ModelVertex()
                {
                    Position = gcmfVtx.Position,
                    Normal = gcmfVtx.Normal,
                    VertexColor = gcmfVtx.VertexColor,
                    PrimaryTexCoord = gcmfVtx.PrimaryTexCoord
                };

                // Apply transformation matrices to the vertex
                if (gcmfVtx.TransformMatrixRef != null && gcmfVtx.TransformMatrixRef.Value != 0)
                {
                    if (gcmfVtx.TransformMatrixRef.Value > 24 || (gcmfVtx.TransformMatrixRef % 3) != 0)
                        throw new InvalidGmaFileException("Invalid TransformMatrixRef for the transform matrix rendering.");
                    int rIdx = (gcmfVtx.TransformMatrixRef.Value / 3) - 1;

                    if (context.TransformMatrixIdxs[rIdx] == byte.MaxValue)
                        throw new InvalidGmaFileException("The transform matrix associated to the matrix reference is not defined.");
                    
                    // Transform the position and normal vectors according to the transform matrix
                    GcmfTransformMatrix tMtx = context.Gcmf.TransformMatrices[context.TransformMatrixIdxs[rIdx]];
                    modelVtx.Position = tMtx.TransformPosition(modelVtx.Position);
                    if (modelVtx.Normal != null)
                        modelVtx.Normal = tMtx.TransformNormal(modelVtx.Normal.Value);
                }

                modelVertices[i] = modelVtx;
            }

            // Write triangle strip
            renderer.WriteTriangleStrip(modelVertices);
        }

        internal bool LoadNonIndexed(EndianBinaryReader input, uint vertexFlags, bool is16Bit)
        {
            byte nonIndexedTypeValue = input.ReadByte();
            if (nonIndexedTypeValue == 0)
                return false;
            if (nonIndexedTypeValue != (uint)GcmfNonIndexedVertexDataType.Uint16 &&
                nonIndexedTypeValue != (uint)GcmfNonIndexedVertexDataType.Float)
            {
                throw new InvalidGmaFileException("Invalid non-indexed triangle strip vertex type.");
            }

            if (is16Bit && nonIndexedTypeValue != (uint)GcmfNonIndexedVertexDataType.Uint16)
                throw new InvalidGmaFileException("GCMF defined as 16bit but vertex doesn't have 16 bit format.");
            else if (!is16Bit && nonIndexedTypeValue == (uint)GcmfNonIndexedVertexDataType.Uint16)
                throw new InvalidGmaFileException("GCMF not defined as 16bit but vertex has 16 bit format.");

            int numVertices = input.ReadUInt16();

            for (int i = 0; i < numVertices; i++)
            {
                GcmfVertex vtx = new GcmfVertex();
                vtx.LoadNonIndexed(input, vertexFlags, is16Bit);
                Items.Add(vtx);
            }

            return true;
        }

        internal int SizeOfNonIndexed(bool is16Bit)
        {
            return 3 + Items.Sum(vtx => vtx.SizeOfNonIndexed(is16Bit));
        }

        internal void SaveNonIndexed(EndianBinaryWriter output, bool is16Bit)
        {
            output.Write(is16Bit ? (byte)GcmfNonIndexedVertexDataType.Uint16 : (byte)GcmfNonIndexedVertexDataType.Float);
            output.Write(Convert.ToUInt16(Items.Count));
            foreach (GcmfVertex vtx in Items)
                vtx.SaveNonIndexed(output, is16Bit);
        }

        internal int LoadIndexed(EndianBinaryReader input, OrderedSet<GcmfVertex> vertexPool, uint vertexFlags)
        {
            int nIntsRead = 0;

            int stripLength = input.ReadInt32();
            nIntsRead++;

            for (int i = 0; i < stripLength; i++)
            {
                int vertexOff = input.ReadInt32();
                nIntsRead++;

                if ((vertexOff % 0x40) != 0)
                {
                    throw new InvalidGmaFileException("[GcmfTriangleStripType2] vertexOff not multiple of 0x40.");
                }

                int vertexIdx = vertexOff / 0x40;
                if (vertexIdx >= vertexPool.Count)
                {
                    throw new InvalidGmaFileException("[GcmfTriangleStripType2] vertexIdx out of range.");
                }

                vertexPool[vertexIdx].AssignVertexFlagsIndexed(vertexFlags);
                Items.Add(vertexPool[vertexIdx]);
            }

            return nIntsRead;
        }

        internal int SizeOfIndexed()
        {
            return 4 + 4 * Items.Count;
        }

        internal void SaveIndexed(EndianBinaryWriter output, Dictionary<GcmfVertex, int> vertexPoolIndexes)
        {
            output.Write(Items.Count);
            foreach (GcmfVertex vtx in Items)
            {
                int index;
                if (!vertexPoolIndexes.TryGetValue(vtx, out index))
                    throw new InvalidGmaFileException("Indexed triangle strip has a vertex not in the vertex pool.");

                output.Write(index * 0x40);
            }
        }
    }
}
