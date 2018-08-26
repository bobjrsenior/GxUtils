using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LibGxFormat.ModelRenderer
{
    class ObjMtlRenderer : IRenderer, IDisposable
    {
        /// <summary>Output stream for the .OBJ file.</summary>
        StreamWriter objStream;
        /// <summary>Output stream for the .MTL file.</summary>
        StreamWriter mtlStream;

        /// <summary>Current hierachy tree of nested objects.</summary>
        Stack<string> currentObjectHierachy;

        /// <summary>Index of the next vertex coordinate assignable in the .OBJ file.</summary>
        int currentVertexCoordinateIdx;
        /// <summary>Index of the next vertex normal assignable in the .OBJ file.</summary>
        int currentVertexNormalIdx;
        /// <summary>Index of the next texture coordinate assignable in the .OBJ file.</summary>
        int currentVertexTexCoordIdx;

        /// <summary>Index of the next material assignable in the .MTL file.</summary>
        int currentMaterialIdx;

        /// <summary>Map from material identifiers, to .MTL material indices.</summary>
        public Dictionary<int, int> materialIdToMtlMaterialIdx;

        /// <summary>Current vertex direction used to determine if a face is front-facing or not.</summary>
        FrontFaceDirection frontFaceDirection;

        /// <summary>
        /// Create a new .OBJ/.MTL renderer writing to the specified files.
        /// </summary>
        /// <param name="objFileName">The path of the .OBJ file to write.</param>
        /// <param name="mtlFileName">The path of the .MTL file to write.</param>
        public ObjMtlRenderer(string objOutputPath, string filename)
        {
            try
            {
                objStream = new StreamWriter(Path.Combine(objOutputPath, filename + ".obj"));
                mtlStream = new StreamWriter(Path.Combine(objOutputPath, filename + ".mtl"));

                // Write .mtl file reference in the .obj file
                objStream.WriteLine("mtllib {0}", filename + ".mtl");

                // Create empty material for when we want to unbind the current material
                mtlStream.WriteLine("newmtl MAT_NULL");
                mtlStream.WriteLine();

                currentObjectHierachy = new Stack<string>();

                currentVertexCoordinateIdx = 1;
                currentVertexNormalIdx = 1;
                currentVertexTexCoordIdx = 1;

                currentMaterialIdx = 1;
                materialIdToMtlMaterialIdx = new Dictionary<int, int>();
            }
            catch
            {
                Dispose(); // Make sure to close all streams if creation fails
                throw;
            }
        }

        ~ObjMtlRenderer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clear up managed resources
                if (objStream != null)
                {
                    objStream.Dispose();
                    objStream = null;
                }

                if (mtlStream != null)
                {
                    mtlStream.Dispose();
                    mtlStream = null;
                }
            }
        }

        public void ClearMaterialList()
        {
            materialIdToMtlMaterialIdx.Clear();
        }

        public void DefineMaterial(int materialId, int textureId, TextureWrapMode wrapModeS, TextureWrapMode wrapModeT)
        {
            // TODO implement wrap mode - think it's not supported in MTL :(
            mtlStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "newmtl gxmat{0}", currentMaterialIdx));
            mtlStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "map_Kd {0}.png", textureId));
            mtlStream.WriteLine();

            materialIdToMtlMaterialIdx[materialId] = currentMaterialIdx;

            currentMaterialIdx++;
        }

        public void BindMaterial(int materialId)
        {
            if (!materialIdToMtlMaterialIdx.ContainsKey(materialId))
                throw new InvalidOperationException("Attempting to bind a material not previously defined.");

            objStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "usemtl gxmat{0}", materialIdToMtlMaterialIdx[materialId]));
        }

        public void UnbindMaterial()
        {
            objStream.WriteLine("usemtl MAT_NULL");
        }

        public void BeginObject(string objectName)
        {
            // The .OBJ format supports only one level of nesting, while we support as many as desired
            // Write the non-top level hierachy as a real .OBJ object definition and the rest as comments
            if (currentObjectHierachy.Count == 0)
            {
                objStream.WriteLine("o {0}", objectName);
            }
            else
            {
                objStream.WriteLine("# o {0}", objectName);
            }

            currentObjectHierachy.Push(objectName);
        }

        public void EndObject()
        {
            string objectName = currentObjectHierachy.Pop();

            // Just a helpful comment, nothing special about this
            objStream.WriteLine("# End o {0}", objectName);
        }

        public void SetFrontFaceDirection(FrontFaceDirection frontFaceDirection)
        {
            this.frontFaceDirection = frontFaceDirection;
        }

        public void SetTwoSidedFaces(bool enable)
        {
            // Not supported
        }

        private struct ObjVertexIdxs
        {
            public int CoordIdx;
            public int NormalIdx;
            public int TexCoordIdx;
        }
        
        public void WriteTriangleStrip(IList<ModelVertex> triangleStrip)
        {
            // Write vertex data to .OBJ file
            List<ObjVertexIdxs> vertexIdxs = new List<ObjVertexIdxs>();
            foreach (ModelVertex vtx in triangleStrip)
            {
                vertexIdxs.Add(new ObjVertexIdxs() {
                    CoordIdx = WriteVertexCoordinate(vtx),
                    NormalIdx = WriteVertexNormal(vtx),
                    TexCoordIdx = WriteVertexTexCoord(vtx)
                });
            }

            // Write triangle indices to OBJ file
            // Convert triangle strips to individual triangles
            // http://en.wikipedia.org/wiki/Triangle_strip
            for (int i = 0; i < triangleStrip.Count - 2; i++)
            {
                int v1, v2, v3;

                if (((i % 2) == 0) ^ (frontFaceDirection == FrontFaceDirection.Cw))
                {
                    v1 = i + 0;
                    v2 = i + 1;
                    v3 = i + 2;
                }
                else
                {
                    v1 = i + 1;
                    v2 = i + 0;
                    v3 = i + 2;
                }

                WriteFace(vertexIdxs[v1], vertexIdxs[v2], vertexIdxs[v3]);
            }
        }

        private int WriteVertexCoordinate(ModelVertex vertex)
        {
            objStream.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "v {0} {1} {2}", vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
            return currentVertexCoordinateIdx++;
        }

        private int WriteVertexNormal(ModelVertex vertex)
        {
            if (!vertex.Normal.HasValue)
                return 0;

            objStream.WriteLine(string.Format(CultureInfo.InvariantCulture, "vn {0} {1} {2}",
                vertex.Normal.Value.X, vertex.Normal.Value.Y, vertex.Normal.Value.Z));
            return currentVertexNormalIdx++;
        }

        private int WriteVertexTexCoord(ModelVertex vertex)
        {
            if (!vertex.PrimaryTexCoord.HasValue)
                return 0;

            /* OBJ considers (0,0) to be the top left, GMA and OpenGL consider it to be the bottom left.
             * See http://stackoverflow.com/a/5605027 (Thanks to Tommy @ StackOverflow). */
            objStream.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                "vt {0} {1}", vertex.PrimaryTexCoord.Value.X, (1 - vertex.PrimaryTexCoord.Value.Y)));
            return currentVertexTexCoordIdx++;
        }

        private void WriteFace(ObjVertexIdxs v1, ObjVertexIdxs v2, ObjVertexIdxs v3)
        {
            if (v1.NormalIdx != 0 && v2.NormalIdx != 0 && v3.NormalIdx != 0)
            {
                if (v1.TexCoordIdx != 0 && v2.TexCoordIdx != 0 && v3.TexCoordIdx != 0)
                {
                    // Vertex+TexCoord+Normal
                    objStream.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                        "f {0}/{1}/{2} {3}/{4}/{5} {6}/{7}/{8}",
                        v1.CoordIdx ,v1.TexCoordIdx, v1.NormalIdx,
                        v2.CoordIdx ,v2.TexCoordIdx, v2.NormalIdx,
                        v3.CoordIdx ,v3.TexCoordIdx, v3.NormalIdx));
                }
                else
                {
                    // Vertex+Normal
                    objStream.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                        "f {0}//{1} {2}//{3} {4}//{5}",
                        v1.CoordIdx, v1.NormalIdx,
                        v2.CoordIdx, v2.NormalIdx,
                        v3.CoordIdx, v3.NormalIdx));
                }
            }
            else
            {
                if (v1.TexCoordIdx != 0 && v2.TexCoordIdx != 0 && v3.TexCoordIdx != 0)
                {
                    // Vertex+TexCoord
                    objStream.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                        "f {0}/{1} {2}/{3} {4}/{5}",
                        v1.CoordIdx, v1.TexCoordIdx,
                        v2.CoordIdx, v2.TexCoordIdx,
                        v3.CoordIdx, v3.TexCoordIdx));
                }
                else
                {
                    // Vertex
                    objStream.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "f {0} {1} {2}", v1.CoordIdx, v2.CoordIdx, v3.CoordIdx));
                }
            }
        }

    }
}
