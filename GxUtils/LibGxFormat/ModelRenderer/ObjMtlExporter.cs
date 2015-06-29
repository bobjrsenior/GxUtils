using System;
using System.Drawing;
using System.IO;

namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Manages the exportation of a set of textures and models to .OBJ / .MTL files.
    /// </summary>
    public class ObjMtlExporter
    {
        /// <summary>
        /// The path that will contain the output files.
        /// </summary>
        string outputPath;

        /// <summary>
        /// Create a new ObjMtlExporter.
        /// </summary>
        /// <param name="outputPath">The path where the output files will be written.</param>
        public ObjMtlExporter(string outputPath)
        {
            if (outputPath == null)
                throw new ArgumentNullException("outputPath");

            this.outputPath = outputPath;
        }

        /// <summary>
        /// Exports the specified texture to a .PNG file.
        /// </summary>
        /// <param name="textureId">An identifier for the texture, referenced from the model.</param>
        /// <param name="tex">The texture to export</param>
        public void ExportTexture(int textureId, ITexture tex)
        {
            if (tex == null)
                throw new ArgumentNullException("tex");

            if (tex.LevelCount != 0) // Texture has texture data
            {
                Bitmap bmp = tex.DecodeLevelToBitmap(0);
                bmp.Save(Path.Combine(outputPath, string.Format("{0}.png", textureId)));
            }
        }

        /// <summary>
        /// Exports the specified model to a .OBJ / .MTL file pair.
        /// </summary>
        /// <param name="model">The model to export.</param>
        public void ExportModel(IModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            string objFileName = Path.Combine(outputPath, "model.obj");
            string mtlFileName = Path.Combine(outputPath, "model.mtl");

            using (ObjMtlRenderer renderer = new ObjMtlRenderer(objFileName, mtlFileName))
            {
                model.Render(renderer);
            }
        }
    }
}
