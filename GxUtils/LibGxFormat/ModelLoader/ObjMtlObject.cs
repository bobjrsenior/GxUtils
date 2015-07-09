using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// An object in a .OBJ / .MTL file pair.
    /// The object is represented as a collection of meshes.
    /// Meshes are created for each material (declared by the "usemtl [material name]" keyword).
    /// Faces which are not included inside any mesh are places in the default mesh.
    /// </summary>
    public class ObjMtlObject
    {
        /// <summary>
        /// The meshes contained within this object.
        /// </summary>
        public Collection<ObjMtlMesh> Meshes
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a new empty object with the specified name.
        /// </summary>
        public ObjMtlObject()
        {
            Meshes = new NonNullableCollection<ObjMtlMesh>();
        }
    }
}
