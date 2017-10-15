using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace LibGxFormat.ModelLoader
{
    /// <summary>
    /// Class used to load .OBJ / .MTL files.
    /// </summary>
    class ObjMtlLoader
    {
        /// <summary>The model in which the .OBJ / .MTL file is loaded into.</summary>
        ObjMtlModel model;

        /// <summary>List of warnings while loading the .OBJ / .MTL file.</summary>
        List<string> warningLog;

        /// <summary>Current .OBJ file path.</summary>
        string objPath;
        /// <summary>Current .OBJ file parser instance.</summary>
        ObjMtlParser objParser;

        /// <summary>Contains the vertex positions loaded from the .OBJ file, in order.</summary>
        List<Vector3> vertexPositions;
        /// <summary>Contains the vertex normals loaded from the .OBJ file, in order.</summary>
        List<Vector3> vertexNormals;
        /// <summary>Contains the vertex texture coordinates loaded from the .OBJ file, in order.</summary>
        List<Vector2> vertexTexCoords;

        /// <summary>While parsing the .OBJ file, the object that is currently being loaded.</summary>
        ObjMtlObject currentObject;
        /// <summary>While parsing the .OBJ file, the mesh that is currently being loaded.</summary>
        ObjMtlMesh currentMesh;
        /// <summary>While parsing the .OBJ file, the material that is currently used.</summary>
        ObjMtlMaterial currentUsedMaterial;

        /// <summary>Current .MTL file path.</summary>
        string mtlPath;
        /// <summary>Current .MTL file parser instance.</summary>
        ObjMtlParser mtlParser;

        /// <summary>Contains the materials loaded from the .MTL file, in order.</summary>
        Dictionary<string, ObjMtlMaterial> materials;

        /// <summary>While parsing the .MTL file, the material that is currently being loaded.</summary>
        ObjMtlMaterial currentLoadMaterial;

        /// <summary>
        /// Create a new .OBJ / .MTL loader.
        /// </summary>
        /// <param name="model">The model in which the files should be loaded into.</param>
        public ObjMtlLoader(ObjMtlModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.model = model;
        }

        /// <summary>
        /// Load the model from the .OBJ file
        /// </summary>
        /// <param name="objPath">The path of a file containing the .OBJ file.</param>
        /// <returns>A list of warnings while loading the file.</returns>
        public List<string> LoadObj(string objPath)
        {
            if (objPath == null)
                throw new ArgumentNullException("objPath");

            // Set up the parser status to load a new .OBJ file
            this.warningLog = new List<string>();

            this.objPath = objPath;
            this.objParser = null;

            this.vertexPositions = new List<Vector3>();
            this.vertexNormals = new List<Vector3>();
            this.vertexTexCoords = new List<Vector2>();

            this.currentObject = null;
            this.currentMesh = null;
            this.currentUsedMaterial = null;

            this.mtlPath = null;
            this.mtlParser = null;

            this.materials = new Dictionary<string, ObjMtlMaterial>();

            this.currentLoadMaterial = null;

            using (objParser = new ObjMtlParser(objPath))
            {
                while (objParser.ReadNextLine())
                {
                    // Check for empty and comment lines
                    if (!objParser.AdvanceToNextNonWhiteSpace())
                        continue;
                    if (objParser.PeekCharacter() == '#')
                        continue;

                    // Parse according to the keyword
                    string keyword = objParser.GetNextWord();

                    switch (keyword)
                    {
                        case "o":
                            ParseObjObjectDeclaration();
                            break;
                        case "mtllib":
                            ParseObjMtlLibDeclaration();
                            break;
                        case "usemtl":
                            ParseObjUseMtlDeclaration();
                            break;
                        case "v":
                            ParseObjVertexPositionDeclaration();
                            break;
                        case "vn":
                            ParseObjVertexNormalDeclaration();
                            break;
                        case "vt":
                            ParseObjVertexTexCoordDeclaration();
                            break;
                        case "f":
                            ParseObjFaceDeclaration();
                            break;
                        default:
                            warningLog.Add(string.Format(
                                "{0}: Unrecognized keyword '{1}'.", objParser.GetFilePositionStr(), keyword));
                            break;
                    }
                }
            }

            return warningLog;
        }

        /// <summary>
        /// Parses a object name declaration in a .OBJ file (a line of the kind "o [object name]").
        /// </summary>
        private void ParseObjObjectDeclaration()
        {
            // Check that the line contains an object name
            if (objParser.IsEndOfLine)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Object declaration without a corresponding object name.", objParser.GetFilePositionStr()));
            }

            // Create the new object instance
            string newObjectName = objParser.ReadRestOfLine().Trim();
            currentObject = new ObjMtlObject();
            currentMesh = null;

            // Add the object to the object list
            if (model.Objects.ContainsKey(newObjectName))
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Duplicate object name '{1}'.", objParser.GetFilePositionStr(), newObjectName));
            }
            model.Objects.Add(newObjectName, currentObject);
        }

        /// <summary>
        /// Parse a material library declaration in a .OBJ file (a line of the kind "mtllib file.mtl").
        /// </summary>
        private void ParseObjMtlLibDeclaration()
        {
            // Check that the line contains a material library name
            if (objParser.IsEndOfLine)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Material library declaration without a corresponding file name.", objParser.GetFilePositionStr()));
            }

            // Load the associated material library
            string materialLibraryName = objParser.ReadRestOfLine().Trim();
            string materialLibraryPath = Path.Combine(Path.GetDirectoryName(objPath), materialLibraryName);
            LoadMtl(materialLibraryPath);

            // Since the old material library should no longer be loaded, unload the old used material
            currentUsedMaterial = null;
        }

        /// <summary>
        /// Parse a material library declaration in a .OBJ file (a line of the kind "mtllib file.mtl").
        /// </summary>
        private void ParseObjUseMtlDeclaration()
        {
            // Check that the line contains a material name
            if (objParser.IsEndOfLine)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Material usage declaration without a corresponding material name.", objParser.GetFilePositionStr()));
            }

            // Load the associated material library
            string materialName = objParser.ReadRestOfLine().Trim();
            
            if (!materials.ContainsKey(materialName))
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Can't find the material '{1}'.", objParser.GetFilePositionStr(), materialName));
            }

            currentUsedMaterial = materials[materialName];

            // Create a new mesh since the material has changed
            currentMesh = null;
        }

        /// <summary>
        /// Parse a vertex position declaration in a .OBJ file (a line of the kind "v 1.0 1.0 1.0").
        /// </summary>
        private void ParseObjVertexPositionDeclaration()
        {
            List<float> position = ReadFloatListFromObj();
            if (position.Count != 3)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Expected vertex position to contain three numbers.", objParser.GetFilePositionStr()));
            }

            vertexPositions.Add(new Vector3(position[0], position[1], position[2]));
        }

        /// <summary>
        /// Parse a vertex normal declaration in a .OBJ file (a line of the kind "vn 1.0 1.0 1.0").
        /// </summary>
        private void ParseObjVertexNormalDeclaration()
        {
            List<float> normal = ReadFloatListFromObj();
            if (normal.Count != 3)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Expected vertex normal to contain three numbers.", objParser.GetFilePositionStr()));
            }

            vertexNormals.Add(new Vector3(normal[0], normal[1], normal[2]));
        }

        /// <summary>
        /// Parse a texture coordinate declaration (a line of the kind "vt 1.0 1.0").
        /// </summary>
        private void ParseObjVertexTexCoordDeclaration()
        {
            List<float> texCoord = ReadFloatListFromObj();
            if (texCoord.Count != 2)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Expected texture coordinate to contain two numbers.", objParser.GetFilePositionStr()));
            }

            vertexTexCoords.Add(new Vector2(texCoord[0], texCoord[1]));
        }

        /// <summary>
        /// Parse a face declaration in a .OBJ file (a line of the kind "f 1 2 3").
        /// </summary>
        private void ParseObjFaceDeclaration()
        {
            ObjMtlFace face = new ObjMtlFace();
            for (string word = objParser.GetNextWord(); word != null; word = objParser.GetNextWord())
            {
                // There are four accepted formats for the face declarations:
                // Position, No Normal, No TexCoord: f POS
                // Position, TexCoord, No Normal: f POS/TEXCOORD
                // Position, No TexCoord, Normal: f POS//NORMAL
                // Position, TexCoord, Normal: f POS/TEXCOORD/NORM

                // Also, take note that the vertex indexes start are one-based and not zero-based

                // Split into components
                string[] vertexIndexStrings = word.Split('/');
                if (vertexIndexStrings.Length > 3)
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Invalid face declaration format (too many components).", objParser.GetFilePositionStr()));
                }

                // Validate valid format specification
                if (vertexIndexStrings[0] == "")
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Invalid face declaration format (vertex position index can't be empty).", objParser.GetFilePositionStr()));
                }

                if (vertexIndexStrings.Length == 2 && vertexIndexStrings[1] == "")
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Invalid face declaration format (texture coordinate index can't be empty unless followed by vertex normal index).", objParser.GetFilePositionStr()));
                }

                if (vertexIndexStrings.Length == 3 && vertexIndexStrings[2] == "")
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Invalid face declaration format (texture normal index can't be empty).", objParser.GetFilePositionStr()));
                }

                // Parse vertex position index (always the first component, always present)
                int vertexPositionIdx;
                if (!int.TryParse(vertexIndexStrings[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out vertexPositionIdx))
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Invalid face declaration format (vertex position index not an integer).", objParser.GetFilePositionStr()));
                }

                if (vertexPositionIdx-1 < 0 || vertexPositionIdx-1 >= vertexPositions.Count)
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Invalid face declaration format (vertex position index out of bounds).", objParser.GetFilePositionStr()));
                }

                // Parse vertex texture coordinate (always the third component, may not be present)
                int vertexTexCoordIdx = -1;
                if (vertexIndexStrings.Length >= 2 && vertexIndexStrings[1] != "")
                {
                    if (!int.TryParse(vertexIndexStrings[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out vertexTexCoordIdx))
                    {
                        throw new InvalidObjMtlFileException(string.Format(
                            "{0}: Invalid face declaration format (vertex texture coordinate index not an integer).", objParser.GetFilePositionStr()));
                    }

                    if (vertexTexCoordIdx - 1 < 0 || vertexTexCoordIdx - 1 >= vertexTexCoords.Count)
                    {
                        throw new InvalidObjMtlFileException(string.Format(
                            "{0}: Invalid face declaration format (vertex texture coordinate index out of bounds).", objParser.GetFilePositionStr()));
                    }
                }

                // Parse vertex normal index (always the second component, may not be present)
                int vertexNormalIdx = -1;
                if (vertexIndexStrings.Length == 3)
                {
                    if (!int.TryParse(vertexIndexStrings[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out vertexNormalIdx))
                    {
                        throw new InvalidObjMtlFileException(string.Format(
                            "{0}: Invalid face declaration format (vertex normal index not an integer).", objParser.GetFilePositionStr()));
                    }

                    if (vertexNormalIdx - 1 < 0 || vertexNormalIdx - 1 >= vertexNormals.Count)
                    {
                        throw new InvalidObjMtlFileException(string.Format(
                            "{0}: Invalid face declaration format (vertex normal index out of bounds).", objParser.GetFilePositionStr()));
                    }
                }

                ObjMtlVertex vtx = new ObjMtlVertex(
                    vertexPositions[vertexPositionIdx - 1],
                    (vertexNormalIdx != -1) ? (Vector3?)vertexNormals[vertexNormalIdx - 1] : null,
                    (vertexTexCoordIdx != -1) ? (Vector2?)vertexTexCoords[vertexTexCoordIdx - 1] : null);

                // Check that all the faces have the same elements
                if (face.Count != 0)
                {
                    if (face[0].Normal.HasValue != vtx.Normal.HasValue ||
                        face[0].TexCoord.HasValue != vtx.TexCoord.HasValue)
                    {
                        throw new InvalidObjMtlFileException(string.Format(
                            "{0}: Invalid face declaration format (not all elements have the same components).", objParser.GetFilePositionStr()));
                    }
                }

                // Add the vertex to the face
                face.Add(vtx);
            }

            // Validate the number of vertices in the face
            if (face.Count < 3)
            {
                throw new InvalidObjMtlFileException(string.Format(
                        "{0}: A face must have at least 3 vertices.", objParser.GetFilePositionStr()));
            }

            // If no object is currently initialized (this happens if the face is outside any object),
            // we add the face to an object named as the empty string.
            if (currentObject == null)
            {
                if (!model.Objects.ContainsKey(""))
                    model.Objects.Add("", new ObjMtlObject());

                currentObject = model.Objects[""];
            }

            // If no mesh is currently initialized (this happens if the face is the first one
            // within the current material), start a new material
            if (currentMesh == null)
            {
                // If no material is used (this happens if the face is before any usemtl declaration),
                // create a new empty material for the face
                if (currentUsedMaterial == null)
                    currentUsedMaterial = new ObjMtlMaterial();

                currentMesh = new ObjMtlMesh(currentUsedMaterial);
                currentObject.Meshes.Add(currentMesh);
            }

            // Add the face to the mesh
            currentMesh.Faces.Add(face);
        }

        /// <summary>
        /// Get the rest of the words in the current .OBJ file line as a list of floating point numbers.
        /// </summary>
        /// <returns>The list of floating point numbers.</returns>
        private List<float> ReadFloatListFromObj()
        {
            List<float> floatList = new List<float>();
            for (string word = objParser.GetNextWord(); word != null; word = objParser.GetNextWord())
            {
                float f;
                if (!float.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                {
                    throw new InvalidObjMtlFileException(string.Format(
                        "{0}: Expected floating point number.", objParser.GetFilePositionStr()));
                }

                floatList.Add(f);
            }
            return floatList;
        }

        /// <summary>
        /// Load a .MTL file from the specified path.
        /// </summary>
        /// <param name="mtlPath">The path of the .MTL file.</param>
        private void LoadMtl(string mtlPath)
        {
            this.mtlPath = mtlPath;
            this.mtlParser = null;

            this.materials = new Dictionary<string, ObjMtlMaterial>();

            this.currentLoadMaterial = null;

            using (mtlParser = new ObjMtlParser(mtlPath))
            {
                while (mtlParser.ReadNextLine())
                {
                    // Check for empty and comment lines
                    if (!mtlParser.AdvanceToNextNonWhiteSpace())
                        continue;
                    if (mtlParser.PeekCharacter() == '#')
                        continue;

                    // Parse according to the keyword
                    string keyword = mtlParser.GetNextWord();

                    switch (keyword)
                    {
                        case "newmtl":
                            ParseMtlMaterialDeclaration();
                            break;
                        case "map_Kd":
                            ParseMtlDiffuseTextureMapDeclaration();
                            break;
                        default:
                            warningLog.Add(string.Format(
                                "{0}: Unrecognized keyword '{1}'.", mtlParser.GetFilePositionStr(), keyword));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Parse a material declaration in a .MTL file (a line of the kind "newmtl testmtl").
        /// </summary>
        private void ParseMtlMaterialDeclaration()
        {
            // Check that the line contains a material name
            if (mtlParser.IsEndOfLine)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Material declaration without a corresponding material name.", mtlParser.GetFilePositionStr()));
            }

            // Create the new material instance
            string materialName = mtlParser.ReadRestOfLine().Trim();
            currentLoadMaterial = new ObjMtlMaterial();
            
            if (materials.ContainsKey(materialName))
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Duplicate material name '{1}'.", mtlParser.GetFilePositionStr(), materialName));
            }
            materials.Add(materialName, currentLoadMaterial);
        }

        /// <summary>
        /// Parse a diffuse texture map in a .MTL file (a line of the kind "Map_Kd test.png").
        /// </summary>
        private void ParseMtlDiffuseTextureMapDeclaration()
        {
            // Check that a material declaration was started
            if (currentLoadMaterial == null)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Texture map declaration before starting a material.", mtlParser.GetFilePositionStr()));
            }

            // Check that the line contains a material name
            mtlParser.AdvanceToNextNonWhiteSpace();
            if (mtlParser.IsEndOfLine)
            {
                throw new InvalidObjMtlFileException(string.Format(
                    "{0}: Texture map declaration without a corresponding texture name.", mtlParser.GetFilePositionStr()));
            }

            // Load the associated texture
            string textureFileName = mtlParser.ReadRestOfLine().Trim();
            string textureFilePath = Path.Combine(Path.GetDirectoryName(mtlPath), textureFileName);

            // https://stackoverflow.com/a/8701748
            Bitmap tempImage;
            using(var bmpOnDisk = new Bitmap(textureFilePath))
            {
                tempImage = new Bitmap(bmpOnDisk);
            }
            currentLoadMaterial.DiffuseTextureMap = new Bitmap(tempImage);
        }
    }
}
