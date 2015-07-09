using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// Temporary information used during the rendering process of a GCMF model.
    /// </summary>
    class GcmfRenderContext
    {
        /// <summary>
        /// GCMF model being rendered.
        /// </summary>
        public Gcmf Gcmf { get; private set; }

        /// <summary>
        /// Transform matrix being used for the current mesh.
        /// Those are updated both when rendering a GCMF model and when rendering a GCMF mesh.
        /// </summary>
        public byte[] TransformMatrixIdxs { get; private set; }

        public GcmfRenderContext(Gcmf gcmf)
        {
            this.Gcmf = gcmf;
            this.TransformMatrixIdxs = new byte[8];
            for (int i = 0; i < TransformMatrixIdxs.Length; i++)
                TransformMatrixIdxs[i] = byte.MaxValue;
        }
    }
}
