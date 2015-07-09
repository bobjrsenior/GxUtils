using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Contains information required to display a model.
    /// </summary>
    public class OpenGlModelContext : IDisposable
    {
        /// <summary>
        /// Map from texture identifiers, to OpenGL texture handles
        /// </summary>
        Dictionary<int, int> textureIdToGlTextureHandle = new Dictionary<int, int>();

        /// <summary>
        /// Display lists created by this model context, in order to clean them up later.
        /// </summary>
        List<Tree<OpenGlModelObjectInformation>> objectModels = new List<Tree<OpenGlModelObjectInformation>>();

        /// <summary>
        /// Load the texture specified by "tex" with the identifier "textureId".
        /// </summary>
        public void SetTexture(int textureId, ITexture tex)
        {
            // Clear loaded texture
            if (textureIdToGlTextureHandle.ContainsKey(textureId))
            {
                GL.DeleteTexture(textureIdToGlTextureHandle[textureId]);
                textureIdToGlTextureHandle.Remove(textureId);
            }

            // Load new texture
            if (tex.LevelCount != 0)
            {
                int texIdx = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texIdx);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                int textureLevelsToUse = Math.Max(1, tex.LevelCount - 3);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, textureLevelsToUse - 1); // Range is inclusive
                for (int level = 0; level < textureLevelsToUse; level++)
                {
                    int dataRowLength = (tex.Width >> level);
                    byte[] data = tex.DecodeLevelToRGBA8(level, dataRowLength * 4);
                    GL.PixelStore(PixelStoreParameter.UnpackRowLength, dataRowLength);
                    GL.TexImage2D(TextureTarget.Texture2D, level, PixelInternalFormat.Rgba, tex.Width >> level, tex.Height >> level,
                                 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
                textureIdToGlTextureHandle[textureId] = texIdx;
            }
        }

        /// <summary>
        /// Clear all textures.
        /// </summary>
        public void ClearTextures()
        {
            foreach (KeyValuePair<int, int> tex in textureIdToGlTextureHandle)
                GL.DeleteTexture(tex.Value);
            textureIdToGlTextureHandle = new Dictionary<int, int>();
        }

        /// <summary>
        /// Activate the texture with the identifier "textureId" for the next polygons.
        /// </summary>
        public void BindTexture(int textureId)
        {
            // Resolve texture id -> GL texture handle
            if (!textureIdToGlTextureHandle.ContainsKey(textureId))
            {
                Console.WriteLine("WARNING: A texture with the identifier {0} has not been defined.", textureId);
                UnbindTexture();
                return;
            }

            GL.BindTexture(TextureTarget.Texture2D, textureIdToGlTextureHandle[textureId]);
        }

        /// <summary>
        /// Deactivate the currently active texture.
        /// </summary>
        public void UnbindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Start a display list at position "index".
        /// </summary>
        public Tree<OpenGlModelObjectInformation> CreateDisplayList(IRenderable model)
        {
            OpenGlRenderer renderer = new OpenGlRenderer(this);
            renderer.BeginObject("root");
            model.Render(renderer);
            renderer.EndObject();
            return renderer.GetModelTree();
        }

        /// <summary>
        /// Clear all loaded models.
        /// </summary>
        public void ClearDisplayLists()
        {
            foreach (Tree<OpenGlModelObjectInformation> model in objectModels)
                model.Traverse(t => GL.DeleteLists(t.Value.DisplayListIndex, 1));
            objectModels.Clear();
        }

        /// <summary>
        /// Render the model at position "index" in OpenGL.
        /// </summary>
        public void CallDisplayList(OpenGlModelObjectInformation model)
        {
            GL.CallList(model.DisplayListIndex);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OpenGlModelContext()
        {
            Dispose(false);
        }

        public void Dispose(bool disposing)
        {
            // Clear unmanaged resources
            ClearTextures();
            ClearDisplayLists();
        }
    }
}
