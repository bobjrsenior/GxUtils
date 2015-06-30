using OpenTK;
using System.Drawing;

namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Represents a vertex in a model.
    /// </summary>
    public class ModelVertex
    {
        /// <summary>
        /// Get the position of the vertex in space.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the vertex normal vector associated to this vertex, if any. It may not be an unit vector.
        /// </summary>
        public Vector3? Normal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the vertex texture coordinates, if any.
        /// </summary>
        public Vector2? PrimaryTexCoord
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the vertex color associated to this vertex, if any.
        /// </summary>
        public Color? VertexColor
        {
            get;
            set;
        }
    }
}
