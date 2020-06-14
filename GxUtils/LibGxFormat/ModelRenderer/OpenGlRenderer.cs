using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace LibGxFormat.ModelRenderer
{
    class OpenGlRenderer : IRenderer
    {
        /// <summary>OpenGL model context, which contains information about which textures are loaded, etc.</summary>
        OpenGlModelContext modelContext;

        /// <summary>Reference to the "root" object in the model, which contains the whole model and all its sub-objects.</summary>
        Tree<OpenGlModelObjectInformation> modelRoot;

        /// <summary>Object being currently rendered.</summary>
        Tree<OpenGlModelObjectInformation> currentObject;

        /// <summary>
        /// Currently, only objects which are purely containers (only contain subobjects and no commands),
        /// and objects which are purely commands (don't contain any subobjects) are allowed.
        /// this is true only if the current object has any command defined.
        /// Current</summary>
        bool currentObjectHasOwnCommands;

        /// <summary>Structure that contains information about a defined material.</summary>
        struct OpenGlMaterial
        {
           public int TextureId;
           public TextureWrapMode WrapModeS;
           public TextureWrapMode WrapModeT;
        };

        /// <summary>List of defined materials.</summary>
        Dictionary<int, OpenGlMaterial> materialDefs;

        /// <summary>Create a new OpenGlRenderer ready to start rendering a model.</summary>
        /// <param name="modelContext">OpenGL model context instance containing details about the OpenGL state.</param>
        public OpenGlRenderer(OpenGlModelContext modelContext)
        {
            this.modelContext = modelContext;
            this.modelRoot = null;
            this.currentObject = null;
            this.currentObjectHasOwnCommands = false;
            this.materialDefs = new Dictionary<int, OpenGlMaterial>();
        }

        public void BeginObject(string objectName)
        {
            if (modelRoot != null && currentObject == null)
                throw new InvalidOperationException("Attempting to start a root object, when one was already created and finished.");

            if (currentObjectHasOwnCommands)
                throw new InvalidOperationException("Objects containing both own commands and nested objects are not allowed.");

            // Create the display list and the tree node associated with the object
            int objectDisplayList = GL.GenLists(1);
            try
            {
                OpenGlModelObjectInformation objectInfo = new OpenGlModelObjectInformation(objectName, objectDisplayList);
                Tree<OpenGlModelObjectInformation> objectNode = new Tree<OpenGlModelObjectInformation>(objectInfo);

                if (currentObject != null)
                {
                    // Add the new object node as a children of the current object
                    currentObject.Add(objectNode);
                }
                else // (modelRoot = null here)
                {
                    // First object being created, set it as the root node
                    modelRoot = objectNode;
                }

                // Set the new node as the current object and start its display list
                currentObject = objectNode;
                currentObjectHasOwnCommands = false;
            }
            catch
            {
                GL.DeleteLists(objectDisplayList, 1);
                throw;
            }

        }

        public void EndObject()
        {
            if (currentObject == null)
                throw new InvalidOperationException("Called EndObject while not inside an object.");

            // If this object has own commands, finish the display list started by CheckStartOwnDisplayList
            if (currentObjectHasOwnCommands)
            {
                GL.EndList();
            }

            // Otherwise, if this object is purely a container, then generate the display list contain all subobjects
            if (!currentObjectHasOwnCommands && currentObject.Count != 0)
            {
                GL.NewList(currentObject.Value.DisplayListIndex, ListMode.Compile);
                for (int i = 0; i < currentObject.Count; i++)
                {
                    GL.CallList(currentObject[i].Value.DisplayListIndex);
                }
                GL.EndList();
            }

            // Go to the parent object agent
            currentObject = currentObject.Parent;
            currentObjectHasOwnCommands = false;
        }

        /// <summary>
        /// If necessary, starts a new display list to contain the commands of the current object.
        /// </summary>
        private void CheckStartOwnDisplayList()
        {
            if (currentObject.Count != 0)
                throw new InvalidOperationException("Objects containing both own commands and nested objects are not allowed.");

            // If this is the first command on the object, generate its own display list
            if (!currentObjectHasOwnCommands)
            {
                currentObjectHasOwnCommands = true;

                GL.NewList(currentObject.Value.DisplayListIndex, ListMode.Compile);
            }
        }

        /// <summary>
        /// Get the tree containing the information and display list indices of all currently defined models.
        /// Before calling this, the root object must have been defined and finished.
        /// </summary>
        /// <returns>A tree structure containing all objects defined using this rendered.</returns>
        public Tree<OpenGlModelObjectInformation> GetModelTree()
        {
            if (modelRoot == null)
                throw new InvalidOperationException("Called GetModelTree before defining the root object.");
            if (currentObject != null)
                throw new InvalidOperationException("Called GetModelTree before finishing the root object.");

            return modelRoot;
        }

        public void ClearMaterialList()
        {
            materialDefs.Clear();
        }

        public void DefineMaterial(int materialId, int textureId, TextureWrapMode wrapModeS, TextureWrapMode wrapModeT)
        {
            OpenGlMaterial materialDef;
            materialDef.TextureId = textureId;
            materialDef.WrapModeS = wrapModeS;
            materialDef.WrapModeT = wrapModeT;
            materialDefs.Add(materialId, materialDef);
        }

        public void BindMaterial(int materialId)
        {
            if (currentObject == null)
                throw new InvalidOperationException("Called BindMaterial while not inside an object.");
            CheckStartOwnDisplayList();

            // Resolve material id -> texture id
            if (!materialDefs.ContainsKey(materialId))
                throw new InvalidOperationException("Attempted to bind to material " + materialId + ", which was not found in a mesh's material list.");

            OpenGlMaterial mat = materialDefs[materialId];

            modelContext.BindTexture(mat.TextureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)mat.WrapModeS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)mat.WrapModeT);
        }

        public void UnbindMaterial()
        {
            if (currentObject == null)
                throw new InvalidOperationException("Called UnbindMaterial while not inside an object.");
            CheckStartOwnDisplayList();

            modelContext.UnbindTexture();
        }

        public void SetFrontFaceDirection(FrontFaceDirection frontFaceDirection)
        {
            if (currentObject == null)
                throw new InvalidOperationException("Called SetFrontFaceDirection while not inside an object.");
            CheckStartOwnDisplayList();

            GL.FrontFace(frontFaceDirection);
        }

        public void SetTwoSidedFaces(bool enable)
        {
            if (currentObject == null)
                throw new InvalidOperationException("Called SetFrontFaceDirection while not inside an object.");
            CheckStartOwnDisplayList();

            // If enabled, disable backface culling so our face doesn't get discarded for not being front facing
            if (enable)
                GL.Disable(EnableCap.CullFace);
            else
                GL.Enable(EnableCap.CullFace);

            // And enable two sided lighting so the other face is not completely black
            GL.LightModel(LightModelParameter.LightModelTwoSide, enable ? 1 : 0);
        }

        public void WriteTriangleStrip(IList<ModelVertex> triangleStrip)
        {
            if (currentObject == null)
                throw new InvalidOperationException("Called WriteTriangleStrip while not inside an object.");
            CheckStartOwnDisplayList();

            GL.Begin(PrimitiveType.TriangleStrip);

            foreach (ModelVertex vtx in triangleStrip)
            {
                if (vtx.Normal.HasValue)
                    GL.Normal3(vtx.Normal.Value.X, vtx.Normal.Value.Y, vtx.Normal.Value.Z);

                if (vtx.PrimaryTexCoord.HasValue)
                    GL.TexCoord2(vtx.PrimaryTexCoord.Value.X, vtx.PrimaryTexCoord.Value.Y);

                if (vtx.VertexColor.HasValue)
                    GL.Color4(vtx.VertexColor.Value.R, vtx.VertexColor.Value.G, vtx.VertexColor.Value.B, vtx.VertexColor.Value.A);
                else
                    GL.Color4(0xFF, 0xFF, 0xFF, 0xFF); // Reset color to the default value

                GL.Vertex3(vtx.Position.X, vtx.Position.Y, vtx.Position.Z);
            }

            GL.End();
        }
    }
}
