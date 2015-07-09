using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// A model loaded from a .OBJ/.MTL file pair.
    /// The model contains a collection of objects (declared by the "o [object name]" keyword).
    /// Meshes which are not included inside any object can be placed in a object with an empty name.
    /// </summary>
    public class ObjMtlModel
    {
        /// <summary>
        /// The objects contained within the model.
        /// </summary>
        public IDictionary<string, ObjMtlObject> Objects
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a new empty model.
        /// </summary>
        public ObjMtlModel()
        {
            this.Objects = new Dictionary<string, ObjMtlObject>();
        }

        /// <summary>
        /// Load a model from a .OBJ/.MTL file pair.
        /// </summary>
        /// <param name="objPath">The path of a file containing the .OBJ file.</param>
        /// <param name="warningLog">A list of non-fatal warnings while parsing the file.</param>
        public ObjMtlModel(string objPath, out List<string> warningLog) : this()
        {
            ObjMtlLoader loader = new ObjMtlLoader(this);
            warningLog = loader.LoadObj(objPath);
        }
    }
}
