using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibGxFormat.Gma
{
    public class GcmfTriangleStrip
    {
        GcmfNonIndexedVertexDataType nonIndexedType;
        List<GcmfVertex> vertices = new List<GcmfVertex>();

        List<int> vertexIdxs = new List<int>();
        
        internal void Render(IRenderer renderer, GcmfRenderContext context)
        {
            // Convert GcmfVertex list to ModelVertex list
            int numVertices = !context.Gcmf.IsIndexed ? vertices.Count : vertexIdxs.Count;
            ModelVertex[] modelVertices = new ModelVertex[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                GcmfVertex gcmfVtx = !context.Gcmf.IsIndexed ? vertices[i] : context.Gcmf.VertexPool[vertexIdxs[i]];

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

        internal bool LoadNonIndexed(EndianBinaryReader input, uint vertexFlags)
        {
            byte nonIndexedTypeValue = input.ReadByte();
            if (nonIndexedTypeValue == 0)
                return false;

            if (!Enum.IsDefined(typeof(GcmfNonIndexedVertexDataType), nonIndexedTypeValue))
                throw new InvalidGmaFileException("GcmfObjectPart: Invalid triangle strip type.");
            nonIndexedType = (GcmfNonIndexedVertexDataType)nonIndexedTypeValue;

            int numVertices = input.ReadUInt16();

            for (int i = 0; i < numVertices; i++)
            {
                GcmfVertex vtx = new GcmfVertex();
                vtx.LoadNonIndexed(input, nonIndexedType, vertexFlags);
                vertices.Add(vtx);
            }

            return true;
        }

        internal int SizeOfNonIndexed()
        {
            return 3 + vertices.Sum(vtx => vtx.SizeOfNonIndexed(nonIndexedType));
        }

        internal void SaveNonIndexed(EndianBinaryWriter output)
        {
            output.Write((byte)nonIndexedType);
            output.Write(Convert.ToUInt16(vertices.Count));
            foreach (GcmfVertex vtx in vertices)
                vtx.SaveNonIndexed(output, nonIndexedType);
        }

        internal int LoadIndexed(EndianBinaryReader input, IList<GcmfVertex> vertexPool, uint vertexFlags)
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
                vertexIdxs.Add(vertexIdx);
            }

            return nIntsRead;
        }

        internal int SizeOfIndexed()
        {
            return 4+4*vertexIdxs.Count;
        }

        internal void SaveIndexed(EndianBinaryWriter output)
        {
            output.Write(vertexIdxs.Count);
            foreach (int index in vertexIdxs)
                output.Write(index * 0x40);
        }
    }
}
