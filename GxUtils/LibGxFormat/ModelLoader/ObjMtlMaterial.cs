using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// Represents a material declaration in a .MTL file.
    /// </summary>
    public class ObjMtlMaterial
    {
        /// <summary>Diffuse texture map, or null if one is not associated.</summary>
        public Bitmap DiffuseTextureMap
        {
            get;
            set;
        }

        /// <summary>Create a new material with no properties associated.</summary>
        public ObjMtlMaterial()
        {
            this.DiffuseTextureMap = null;
        }
    }
}
