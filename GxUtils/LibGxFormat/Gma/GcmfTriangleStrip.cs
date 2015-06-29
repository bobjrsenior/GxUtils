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
            IList<IVertex> modelVertices;
            if (!context.Gcmf.IsIndexed)
            {
                modelVertices = vertices.Cast<IVertex>().ToList();
            }
            else
            {
                modelVertices = new List<IVertex>();
                foreach (int index in vertexIdxs)
                    modelVertices.Add(context.Gcmf.VertexPool[index]);
            }

            // Transform vertices (TODO: Make less hacky!!!)
            for (int i = 0; i < modelVertices.Count; i++)
            {
                GcmfVertex vtxOld = (GcmfVertex)modelVertices[i];
                GcmfVertex vtxNew = new GcmfVertex();
                vtxNew.Position = vtxOld.Position;
                vtxNew.Normal = vtxOld.Normal;
                vtxNew.PrimaryTexCoord = vtxOld.PrimaryTexCoord;
                vtxNew.VertexColor = vtxOld.VertexColor;
                if (vtxOld.TransformMatrixRef.HasValue)
                {
                    if (vtxOld.TransformMatrixRef.Value > 24 || (vtxOld.TransformMatrixRef % 3) != 0)
                        throw new InvalidGmaFileException("Invalid TransformMatrixRef for the transform matrix hack.");
                    int rIdx = (vtxOld.TransformMatrixRef.Value / 3) - 1;

                    if (rIdx != -1 && context.TransformMatrixIdxs[rIdx] != byte.MaxValue)
                    {
                        GcmfTransformMatrix tMtx = context.Gcmf.TransformMatrices[context.TransformMatrixIdxs[rIdx]];
                        Matrix4 mtx = new Matrix4(tMtx.Matrix.Row0,
                            tMtx.Matrix.Row1, tMtx.Matrix.Row2, new Vector4(0, 0, 0, 1));
                        mtx.Transpose();
                        Vector3 pos = vtxNew.Position;
                        float newX = mtx[0,0] * pos.X + mtx[0,1] * pos.Y + mtx[0,2] * pos.Z + mtx[0,3];
                        float newY = mtx[1,0] * pos.X + mtx[1,1] * pos.Y + mtx[1,2] * pos.Z + mtx[1,3];
                        float newZ = mtx[2,0] * pos.X + mtx[2,1] * pos.Y + mtx[2,2] * pos.Z + mtx[2,3];
                        vtxNew.Position = Vector3.TransformPosition(pos, mtx);
                        if (vtxNew.Normal.HasValue)
                            vtxNew.Normal = Vector3.TransformNormal(vtxNew.Normal.Value, mtx);
                    }
                }
                modelVertices[i] = vtxNew;
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
