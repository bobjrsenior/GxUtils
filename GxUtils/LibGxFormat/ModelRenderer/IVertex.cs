using OpenTK;
using System.Drawing;

namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Generic interface to be implemented by model vertices.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Get the position of the vertex in space.
        /// </summary>
        Vector3 Position
        {
            get;
        }

        /// <summary>
        /// Gets the vertex normal vector associated to this vertex, if any. It may not be an unit vector.
        /// </summary>
        Vector3? Normal
        {
            get;
        }

        /// <summary>
        /// Gets the vertex texture coordinates, if any.
        /// </summary>
        Vector2? PrimaryTexCoord
        {
            get;
        }

        /// <summary>
        /// Gets the vertex color associated to this vertex, if any.
        /// </summary>
        Color? VertexColor
        {
            get;
        }
    }
}
