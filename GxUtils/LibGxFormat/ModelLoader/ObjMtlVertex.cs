using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// Represents each vertex reference in a .OBJ face declaration.
    /// </summary>
    public class ObjMtlVertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The vertex normal of the vertex, or null if the vertex doesn't have a texture coordinate.
        /// </summary>
        public Vector3? Normal { get; set; }

        /// <summary>
        /// The texture coordinate of the vertex, or null if the vertex doesn't have a texture coordinate.
        /// </summary>
        public Vector2? TexCoord { get; set; }

        /// <summary>
        /// Create a new vertex reference in a .OBJ face declaration.
        /// </summary>
        /// <param name="vertexIdx">Index of the vertex in the global .OBJ vertex array.</param>
        /// <param name="normalIdx">Index of the vertex normal in the global .OBJ normal array, or null if the vertex doesn't have a texture coordinate.</param>
        /// <param name="texCoordIdx">Index of the texture coordinate in the global .OBJ normal array, or null if the vertex doesn't have a texture coordinate.</param>
        public ObjMtlVertex(Vector3 position, Vector3? normal, Vector2? texCoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = texCoord;
        }
    }
}
