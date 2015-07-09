using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// Represents a mesh in a .OBJ/.MTL file pair.
    /// </summary>
    public class ObjMtlMesh
    {
        private ObjMtlMaterial materialBackingStorage;

        /// <summary>
        /// The material used for this mesh.
        /// </summary>
        public ObjMtlMaterial Material
        {
            get
            {
                return materialBackingStorage;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                materialBackingStorage = value;
            }
        }

        /// <summary>
        /// The list of faces in the mesh.
        /// </summary>
        public Collection<ObjMtlFace> Faces
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a new empty mesh.
        /// </summary>
        /// <param name="material">The material used for this mesh.</param>
        public ObjMtlMesh(ObjMtlMaterial material)
        {
            this.Material = material;
            this.Faces = new NonNullableCollection<ObjMtlFace>();
        }
    }
}
