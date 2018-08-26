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
        /// The filename for the OBJ and MTL files
        /// </summary>
        string outputFilename;

        /// <summary>
        /// Create a new ObjMtlExporter.
        /// </summary>
        /// <param name="outputPath">The path where the output files will be written.</param>
        public ObjMtlExporter(string outputPath, string outputFilename)
        {
            if (outputPath == null)
                throw new ArgumentNullException("outputPath");
            if (outputFilename == null)
                throw new ArgumentNullException("outputFilename");

            this.outputPath = outputPath;
            this.outputFilename = outputFilename;
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
        public void ExportModel(IRenderable model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            using (ObjMtlRenderer renderer = new ObjMtlRenderer(outputPath, outputFilename))
            {
                model.Render(renderer);
            }
        }

        /// <summary>
        /// Exports the specified model to a .OBJ / .MTL file pair.
        /// </summary>
        /// <param name="model">The model to export.</param>
        public System.Collections.Generic.List<int> ExportModel(IRenderable model, string modelName)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            using (ObjMtlRenderer renderer = new ObjMtlRenderer(outputPath, outputFilename))
            {
                return model.Render(renderer, modelName);
            }
        }
    }
}
