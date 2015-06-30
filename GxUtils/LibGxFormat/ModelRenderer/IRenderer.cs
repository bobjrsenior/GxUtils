using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Base class to be implemented by all model renderers.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>Create a new sub-object inside the current object.</summary>
        /// <param name="objectName">The name of the object to create. This name is not required to be unique.</param>
        void BeginObject(string objectName);

        /// <summary>Finish the current sub-object.</summary>
        void EndObject();

        /// <summary>Clear the list of currently defined materials.</summary>
        void ClearMaterialList();

        /// <summary>Define a new material. It will be associated a successive index.</summary>
        /// <param name="materialId">Identifier associated with the material.</param>
        /// <param name="materialTextureId">Identifier of the texture, defined in the underlying context.</param>
        /// <param name="wrapModeS">S wrapping mode.</param>
        /// <param name="wrapModeT">T wrapping mode.</param>
        void DefineMaterial(int materialId, int materialTextureId, TextureWrapMode wrapModeS, TextureWrapMode wrapModeT);

        /// <summary>Set the specified material as the current material used for future rendering.</summary>
        /// <param name="materialId">Identifier associated with the material.</param>
        void BindMaterial(int materialId);

        /// <summary>Unset the currently binded material.</summary>
        void UnbindMaterial();

        /// <summary>Set the vertex direction that is used to determine is face in a strip is front-facing or not.</summary>
        /// <param name="dir">The direction (clockwise or counterclockwise).</param>
        void SetFrontFaceDirection(FrontFaceDirection dir);

        /// <summary>Enable or disable faces which are shown and lighted on the two sides, not just on the front-facing side.</summary>
        /// <param name="enable">True to enable, false to disable.</param>
        void SetTwoSidedFaces(bool enable);

        /// <summary>Add a triangle strip to the current object geometry.</summary>
        /// <param name="vertexList">The list of vertices in the triangle strip.</param>
        void WriteTriangleStrip(IList<ModelVertex> vertexList);
    }
}
