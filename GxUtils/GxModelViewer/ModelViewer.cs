using GxModelViewer_WinFormsExt;
using LibGxTexture;
using LibGxFormat;
using LibGxFormat.Gma;
using LibGxFormat.ModelRenderer;
using LibGxFormat.Tpl;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LibGxFormat.ModelLoader;
using System.Collections.Generic;

namespace GxModelViewer
{
    public partial class ModelViewer : Form
    {
        struct TextureReference
        {
            /// <summary>The index of the texture in the TPL file.</summary>
            public int TextureIdx;
            /// <summary>Selected mipmap level in the texture, or -1 for the whole texture.</summary>
            public int TextureLevel;

            public TextureReference(int textureIdx, int textureLevel)
            {
                this.TextureIdx = textureIdx;
                this.TextureLevel = textureLevel;
            }
        };

        struct ModelMeshReference
        {
            /// <summary>The index of the model, or the model that the triangle mesh belongs to.</summary>
            public int ModelIdx;
            /// <summary>The index of the mesh, or (size_t)-1 if it is a whole model</summary>
            public int MeshIdx;

            public ModelMeshReference(int modelIdx, int meshIdx)
            {
                this.ModelIdx = modelIdx;
                this.MeshIdx = meshIdx;
            }
        };

        struct ModelMaterialReference
        {
            /// <summary>The index of the model that contains the material</summary>
            public int ModelIdx;
            /// <summary>The index of the material within the model</summary>
            public int MaterialIdx;
            
            public  ModelMaterialReference(int modelIdx, int materialIdx)
            {
                this.ModelIdx = modelIdx;
                this.MaterialIdx = materialIdx;
            }
        };

        /// <summary>Path to the currently loaded .GMA file, or null if there isn't any.</summary>
        string gmaPath;
        /// <summary>Instance of the currently loaded .GMA file, or null if there isn't any.</summary>
        Gma gma;
        /// <summary>true if the .GMA file has been modified and the changes have not yet been saved.</summary>
        bool haveUnsavedGmaChanges;

        /// <summary>Path to the currently loaded .TPL file, or null if there isn't any.</summary>
        string tplPath;
        /// <summary>Instance of the currently loaded .TPL file, or null if there isn't any.</summary>
        Tpl tpl;
        /// <summary>true if the .TPL file has been modified and the changes have not yet been saved.</summary>
        bool haveUnsavedTplChanges;

        /// <summary>true if a .TPL/.GMA has been loaded and the OpenGL references need to be updated.</summary>
        bool reloadOnNextRedraw;
        /// <summary>Manager for the textures and display lists associated with the .GMA/.TPL files.</summary>
        OpenGlModelContext ctx = new OpenGlModelContext();
        
        /// <summary>
        /// A tree containing the objects defined in the currently loaded GMA file.
        /// The first level of tree children contains a node for each GCMF model in the GMA file,
        /// and the second level of tree children contains a node for each mesh in the GCMF model.
        /// </summary>
        Tree<OpenGlModelObjectInformation> modelObjects;

        /// <summary>true if the left mouse button is currently down on the model viewer.</summary>
        bool isMouseDownOnModelControl = false;
        /// <summary>Last observed coordinates of the mouse in the model viewer, to manage mouse events.</summary>
        int modelViewerLastMouseX = 0, modelViewerLastMouseY = 0;

        /// <summary>Angles in the X and Y axis for the model viewer.</summary>
        float angleX = 0.0f, angleY = 0.0f;
        /// <summary>Zoom factor for the model viewer.</summary>
        float zoomFactor = 1.0f;

        /// <summary>The max number of mipmaps to create when importing textures.</summary>
        int numMipmaps = 255;
        /// <summary>Menu items specifying the possible number of mipmaps</summary>
        ToolStripMenuItem[] mipmapItems = new ToolStripMenuItem [10];

        GxInterpolationFormat intFormat = GxInterpolationFormat.NearestNeighbor;
        ToolStripMenuItem[] mipMapIntItems;

        public ModelViewer()
        {
            InitializeComponent();
            glControlModel.MouseWheel += glControlModel_MouseWheel;

            // Make sure right clicking selects a node
            treeModel.NodeMouseClick += (sender, args) => treeModel.SelectedNode = args.Node;

            // Populate ComboBox values from GxGame enum dynamically.
            tsCmbGame.ComboBox.ValueMember = "Key";
            tsCmbGame.ComboBox.DisplayMember = "Value";
            tsCmbGame.ComboBox.DataSource = new BindingSource(Enum.GetValues(typeof(GxGame)).Cast<GxGame>()
                .Select(g => new { Key = g, Value = EnumUtils.GetEnumDescription(g) }).ToArray(), null);

            
            // Populate the Menu Strip for the number of mipmaps
            int i;
            for (i = 0; i < mipmapItems.Length - 1; ++i)
            {
                ToolStripMenuItem item = new ToolStripMenuItem("" + i);
                item.Click += new EventHandler(mipmapDropDownItemClicked);
                mipmapItems[i] = item;
            }

            {
                ToolStripMenuItem item = new ToolStripMenuItem("255");
                item.Click += new EventHandler(mipmapDropDownItemClicked);
                mipmapItems[mipmapItems.Length - 1] = item;
                item.Checked = true;
            }
            numMipmapsToolStripMenuItem.DropDownItems.AddRange(mipmapItems);


            // Populate the Menu Strip for the different interpolation formats
            var interpolationTypes = Enum.GetValues(typeof(GxInterpolationFormat)).Cast<GxInterpolationFormat>();
            mipMapIntItems = new ToolStripMenuItem[interpolationTypes.Count<GxInterpolationFormat>()];

            i = 0;
            foreach(GxInterpolationFormat intFormatEnum in interpolationTypes)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(EnumUtils.GetEnumDescription(intFormatEnum));
                item.Click += new EventHandler(mipmapInterpolationDropDownItemClicked);
                mipMapIntItems[i] = item;
                if(intFormatEnum == intFormat)
                {
                    item.Checked = true;
                }

                ++i;
            }
            mipmapInterpolationToolStripMenuItem.DropDownItems.AddRange(mipMapIntItems);

            LoadGmaFile(null);
            LoadTplFile(null);
        }

        private GxGame GetSelectedGame()
        {
            return (GxGame)tsCmbGame.ComboBox.SelectedValue;
        }

        private GxInterpolationFormat GetSelectedMipmap()
        {
            return intFormat;
        }

        private int GetNumMipmaps()
        {
            return numMipmaps;
        }

        private void UnloadModel()
        {
            ctx.ClearTextures();
            ctx.ClearDisplayLists();
            modelObjects = null;
        }

        private void LoadModel()
        {
            // Make sure that the old model is unloaded correctly
            UnloadModel();

            // TODO ALPHA CHANNEL. Not as easy as just enabling source alpha
            // http://www.opengl.org/resources/faq/technical/transparency.htm
            // See last paragraph of 15.020

            // Load textures
            if (tpl != null)
            {
                for (int i = 0; i < tpl.Count; i++)
                    ctx.SetTexture(i, tpl[i]);
            }

            // Generate OpenGL display lists
            if (gma != null)
            {
                modelObjects = ctx.CreateDisplayList(gma);
            }
        }

        private void glControlModel_Load(object sender, EventArgs e)
        {
            SetupViewport();

            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(Color.DarkGray);
            GL.ClearDepth(1.0);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Normalize);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.Enable(EnableCap.AlphaTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            if (toolStripMenuItemShowTextures.Checked)
                GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
        }

        private void SetupViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, (float)glControlModel.Width / glControlModel.Height, 0.001f, 10000.0f);
            GL.LoadMatrix(ref perspectiveMatrix);

            GL.Viewport(0, 0, glControlModel.Width, glControlModel.Height);
        }

        private void glControlModel_Paint(object sender, PaintEventArgs e)
        {
            // Reload the model if necessary
            if (reloadOnNextRedraw)
            {
                LoadModel();
                reloadOnNextRedraw = false;
            }

            // Set up OpenGL context
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(0.0f, 0.0f, -6.0f);
            GL.Rotate(angleY, 1.0f, 0.0f, 0.0f);
            GL.Rotate(angleX, 0.0f, 1.0f, 0.0f);
            GL.Scale(zoomFactor, zoomFactor, zoomFactor);

            // Draw the models/meshes
            foreach (TreeNode modelItem in treeModel.Nodes)
            {
                ModelMeshReference modelRef = (ModelMeshReference)modelItem.Tag;
                if (gma[modelRef.ModelIdx] != null)
                {
                    if (treeModel.GetCheckState(modelItem) == CheckState.Checked)
                    {
                        // Whole item is checked -> Call the display list corresponding to the entire model
                        ctx.CallDisplayList(modelObjects[modelRef.ModelIdx].Value);
                    }
                    else if (treeModel.GetCheckState(modelItem) == CheckState.Indeterminate)
                    {
                        // Only some meshes of the item are checked -> Call the display list corresponding to each mesh
                        foreach (TreeNode meshItem in modelItem.Nodes)
                        {
                            if (treeModel.GetCheckState(meshItem) == CheckState.Checked)
                            {
                                ModelMeshReference meshRef = (ModelMeshReference)meshItem.Tag;
                                ctx.CallDisplayList(modelObjects[meshRef.ModelIdx][meshRef.MeshIdx].Value);
                            }
                        }
                    }
                }
            }

            glControlModel.SwapBuffers();
        }

        private void glControlModel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDownOnModelControl = true;
                modelViewerLastMouseX = e.X;
                modelViewerLastMouseY = e.Y;
            }
        }

        private void glControlModel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMouseDownOnModelControl == true)
            {
                int deltaX = e.X - modelViewerLastMouseX;
                int deltaY = e.Y - modelViewerLastMouseY;

                angleX += (float)deltaX;
                angleY += (float)deltaY;
                
                modelViewerLastMouseX = e.X;
                modelViewerLastMouseY = e.Y;

                glControlModel.Invalidate();
            }
        }

        private void glControlModel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDownOnModelControl = false;
                modelViewerLastMouseX = 0;
                modelViewerLastMouseY = 0;
            }
        }

        void glControlModel_MouseWheel(object sender, MouseEventArgs e)
        {
            zoomFactor *= (float)Math.Pow(1.5, e.Delta / 120.0);
            glControlModel.Invalidate();
        }

        private void glControlModel_Resize(object sender, EventArgs e)
        {
            SetupViewport();
        }

        private void tsBtnLoadGma_Click(object sender, EventArgs e)
        {
            if (!CheckSaveUnsavedChanges())
                return;

            // Suggest the name associated with the loaded .TPL file if one is loaded
            if (tplPath != null && tplPath.EndsWith(".tpl"))
                ofdLoadGma.FileName = tplPath.Substring(0, tplPath.Length - 3) + "gma";

            // Ask the user for a GMA file
            if (ofdLoadGma.ShowDialog() != DialogResult.OK)
                return;

            LoadGmaFile(ofdLoadGma.FileName);
        }

        private void LoadGmaFile(string newGmaPath)
        {
            // Try to load the GMA file
            if (newGmaPath != null)
            {
                try
                {
                    using (Stream gmaStream = File.OpenRead(newGmaPath))
                    {
                        gma = new Gma(gmaStream, GetSelectedGame());
                        gmaPath = newGmaPath;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    gma = null;
                    gmaPath = null;
                }
            }
            else
            {
                gma = null;
                gmaPath = null;
            }
            haveUnsavedGmaChanges = false;

            // Update model list
            UpdateModelTree();
            UpdateModelButtons();
            UpdateModelDisplay();

            // Update material tab
            UpdateMaterialList();
            UpdateMaterialDisplay();

            // Update model viewer
            reloadOnNextRedraw = true;
            glControlModel.Invalidate();
        }

        private void tsBtnSaveGma_Click(object sender, EventArgs e)
        {
            SaveGmaFile();
        }

        private void tsBtnSaveGmaAs_Click(object sender, EventArgs e)
        {
            if (sfdSaveGma.ShowDialog() == DialogResult.OK)
            {
                gmaPath = sfdSaveGma.FileName;
                SaveGmaFile();
            }
        }

        private bool SaveGmaFile()
        {
            // If there isn't currently any path set (e.g. we've just imported a model),
            // we have to request one to the user
            if (gmaPath == null)
            {
                if (sfdSaveGma.ShowDialog() != DialogResult.OK)
                    return false;

                gmaPath = sfdSaveGma.FileName;
            }

            using (Stream gmaStream = File.OpenWrite(gmaPath))
            {
                gma.Save(gmaStream, GetSelectedGame());
            }

            haveUnsavedGmaChanges = false;
            UpdateModelButtons();
            return true;
        }

        private void UpdateModelTree()
        {
            treeModel.Nodes.Clear();
            if (gma != null)
            {
                for (int i = 0; i < gma.Count; i++)
                {
                    // Add entry corresponding to the whole model
                    TreeNode modelItem = new TreeNode((gma[i] != null) ? gma[i].Name : "Unnamed");
                    modelItem.Tag = new ModelMeshReference(i, -1);
                    modelItem.ForeColor = (gma[i] != null) ? Color.DarkGreen : Color.Red;
                    modelItem.ContextMenuStrip = gmaContextMenuStrip;
                    treeModel.Nodes.Add(modelItem);
                    
                    // Add display list entries for the meshes within the model
                    if (gma[i] != null)
                    {
                        Gcmf model = gma[i].ModelObject;
                        for (int j = 0; j < model.Meshes.Count; j++)
                        {
                            int layerNo = (model.Meshes[j].Layer == GcmfMesh.MeshLayer.Layer1) ? 1 : 2;
                            TreeNode meshItem = new TreeNode(string.Format("[Layer {0}] Mesh {1}", layerNo, j));
                            meshItem.Tag = new ModelMeshReference(i, j);
                            meshItem.ContextMenuStrip = meshMenuStrip;
                            modelItem.Nodes.Add(meshItem);
                        }
                    }
                    
                    treeModel.SetCheckState(modelItem, CheckState.Checked);
                }
            }
        }

        private void UpdateModelButtons()
        {
            tsBtnSaveGma.Enabled = (gma != null && haveUnsavedGmaChanges);
            tsBtnSaveGmaAs.Enabled = (gma != null);

            tsBtnExportObjMtl.Enabled = (gma != null);

            btnModelShowAll.Enabled = (gma != null);
            btnModelShowLayer1.Enabled = (gma != null);
            btnModelShowLayer2.Enabled = (gma != null);
            btnModelHideAll.Enabled = (gma != null);
        }

        private void UpdateModelDisplay()
        {
            // If no item is selected in the list, hide the display completely
            if (treeModel.SelectedNode == null)
            {
                tlpModelDisplay.Visible = false;
                tlpMeshDisplay.Visible = false;
                return;
            }

            // Otherwise, extract the ModelTreeItem structure to get the selected model/mesh
            ModelMeshReference modelMeshReference = (ModelMeshReference)treeModel.SelectedNode.Tag;
            if (gma[modelMeshReference.ModelIdx] == null)
            {
                tlpModelDisplay.Visible = false;
                tlpMeshDisplay.Visible = false;
                return;
            }
            Gcmf model = gma[modelMeshReference.ModelIdx].ModelObject;

            // Show information about the selected model/mesh
            if (modelMeshReference.MeshIdx == -1)
            {
                tlpModelDisplay.Visible = true;
                tlpMeshDisplay.Visible = false;

                lblModelSectionFlags.Text = string.Format("0x{0:X8}", model.SectionFlags);
                lblModelCenter.Text = model.BoundingSphereCenter.ToString();
                lblModelRadius.Text = model.BoundingSphereRadius.ToString();
                lblModelTransformMatrixDefaultReferences.Text = string.Join(",",
                    Array.ConvertAll(model.TransformMatrixDefaultIdxs, b => string.Format("0x{0:X2}", b)));
                lblModelNumTransformMatrices.Text = model.TransformMatrices.Count.ToString();
            }
            else
            {
                GcmfMesh mesh = model.Meshes[modelMeshReference.MeshIdx];

                tlpModelDisplay.Visible = false;
                tlpMeshDisplay.Visible = true;

                lblMeshRenderFlags.Text = string.Format("0x{0:X8} ({1})", (uint)mesh.RenderFlags, EnumUtils.GetEnumFlagsString(mesh.RenderFlags));
                lblMeshUnk4.Text = string.Format("0x{0:X8}", mesh.Unk4);
                lblMeshUnk8.Text = string.Format("0x{0:X8}", mesh.Unk8);
                lblMeshUnkC.Text = string.Format("0x{0:X8}", mesh.UnkC);
                lblMeshUnk10.Text = string.Format("0x{0:X4}", mesh.Unk10);
                lblMeshUnk14.Text = string.Format("0x{0:X4}", mesh.Unk14);
                lblMeshPrimaryMaterialIdx.Text = mesh.PrimaryMaterialIdx.ToString();
                lblMeshSecondaryMaterialIdx.Text = mesh.SecondaryMaterialIdx.ToString();
                lblMeshTertiaryMaterialIdx.Text = mesh.TertiaryMaterialIdx.ToString();
                lblMeshTransformMatrixSpecificReferences.Text = string.Join(",",
                    Array.ConvertAll(mesh.TransformMatrixSpecificIdxsObj1, b => string.Format("0x{0:X2}", b)));
                lblMeshCenter.Text = mesh.BoundingSphereCenter.ToString();
                lblMeshUnk3C.Text = mesh.Unk3C.ToString();
                lblMeshUnk40.Text = string.Format("0x{0:X8}", mesh.Unk40);
            }
        }

        int GetSelectedModelIdx()
        {
            // If no item is selected in the list, return -1
            if (treeModel.SelectedNode == null)
                return -1;

            // Otherwise, extract the model/mesh reference structure and get the model index from there
            ModelMeshReference itemData = (ModelMeshReference)treeModel.SelectedNode.Tag;
            return ((ModelMeshReference)treeModel.SelectedNode.Tag).ModelIdx;
        }

        private GcmfMaterial GetSelectedMaterial()
        {
            // If no item is selected in the list, return nullptr
            if (treeMaterials.SelectedNode == null)
                return null;

            // Otherwise, extract the ModelMaterialReference structure to get the selected model/mesh
            ModelMaterialReference itemData = (ModelMaterialReference)treeMaterials.SelectedNode.Tag;
            return gma[itemData.ModelIdx].ModelObject.Materials[itemData.MaterialIdx];
        }

        private void UpdateMaterialList()
        {
            treeMaterials.Nodes.Clear();

            // Make sure that an item is selected in the model list and it corresponds to a non-null model
            int modelIdx = GetSelectedModelIdx();
            if (modelIdx == -1 || gma[modelIdx] == null)
            {
                return;
            }

            // Populate the material list from the model
            Gcmf model = gma[modelIdx].ModelObject;
            for (int i = 0; i < model.Materials.Count; i++)
            {
                TreeNode materialItem = new TreeNode(string.Format("Material {0}", i));
                materialItem.Tag = new ModelMaterialReference(modelIdx, i);
                materialItem.ContextMenuStrip = materialMenuStrip;
                treeMaterials.Nodes.Add(materialItem);
            }
        }

        private void UpdateMaterialDisplay()
        {
            // If no model or material is selected, make the fields blank
            GcmfMaterial material = GetSelectedMaterial();
            if (material == null)
            {
                pbMaterialTextureImage.Image = null;
                tlpMaterialProperties.Visible = false;
                return;
            }

            tlpMaterialProperties.Visible = true;
            lblMaterialFlags.Text = string.Format("0x{0:X8}", material.Flags);
            lblMaterialTextureIndex.Text = string.Format("{0}", material.TextureIdx);
            lblMaterialUnk6.Text = string.Format("0x{0:X2}", material.Unk6);
            lblMaterialAnisotropyLevel.Text = string.Format("0x{0:X2}", material.AnisotropyLevel);
            lblMaterialUnkC.Text = string.Format("0x{0:X4}", material.UnkC);
            lblMaterialUnk10.Text = string.Format("0x{0:X8}", material.Unk10);

            if (tpl != null && material.TextureIdx < tpl.Count && !tpl[material.TextureIdx].IsEmpty)
            {
                TplTexture tex = tpl[material.TextureIdx];
                pbMaterialTextureImage.Image = tex.DecodeLevelToBitmap(0);
            }
            else
            {
                pbMaterialTextureImage.Image = null;
            }
        }

        /// <summary>
        /// Requests to the user to save unsaved changes in order to advance to the next option.
        /// </summary>
        /// <returns>true if the user has decided to advance (may have saved or not), false if the user wants to cancel the action.</returns>
        private bool CheckSaveUnsavedChanges()
        {
            if (haveUnsavedGmaChanges)
            {
                switch (MessageBox.Show("There are unsaved .GMA file changes. Save them now?",
                    "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        return SaveGmaFile();

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        return false;

                    default:
                        throw new InvalidOperationException("Internal error.");

                }
            }

            if (haveUnsavedTplChanges)
            {
                switch (MessageBox.Show("There are unsaved .TPL file changes. Save them now?",
                    "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        return SaveTplFile();

                    case DialogResult.No:
                        break;

                    case DialogResult.Cancel:
                        return false;

                    default:
                        throw new InvalidOperationException("Internal error.");
                }
            }

            return true;
        }

        private void tsBtnLoadTpl_Click(object sender, EventArgs e)
        {
            if (!CheckSaveUnsavedChanges())
                return;

            // Suggest the name associated with the loaded .GMA file if one is loaded
            if (gmaPath != null && gmaPath.EndsWith(".gma"))
                ofdLoadTpl.FileName = gmaPath.Substring(0, gmaPath.Length - 3) + "tpl";

            // Ask the user for a TPL file
            if (ofdLoadTpl.ShowDialog() != DialogResult.OK)
                return;

            LoadTplFile(ofdLoadTpl.FileName);
        }

        private void LoadTplFile(string newTplPath)
        {
            // Try to load the TPL file
            if (newTplPath != null)
            {
                try
                {
                    using (Stream tplStream = File.OpenRead(newTplPath))
                    {
                        tpl = new Tpl(tplStream, GetSelectedGame());
                        tplPath = newTplPath;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tpl = null;
                    tplPath = null;
                }
            }
            else
            {
                tpl = null;
                tplPath = null;
            }
            haveUnsavedTplChanges = false;

            // Update texture list
            UpdateTextureTree();
            UpdateTextureButtons();
            UpdateTextureDisplay();

            // Update model viewer
            reloadOnNextRedraw = true;
            glControlModel.Invalidate();
        }

        private void tsBtnSaveTpl_Click(object sender, EventArgs e)
        {
            SaveTplFile();
        }

        private void tsBtnSaveTplAs_Click(object sender, EventArgs e)
        {
            if (sfdSaveTpl.ShowDialog() != DialogResult.OK)
                return;

            tplPath = sfdSaveTpl.FileName;
            SaveTplFile();
        }

        private bool SaveTplFile()
        {
            // If there isn't currently any path set (e.g. we've just imported a model),
            // we have to request one to the user
            if (tplPath == null)
            {
                if (sfdSaveTpl.ShowDialog() != DialogResult.OK)
                    return false;

                tplPath = sfdSaveTpl.FileName;
            }

            using (Stream tplStream = File.OpenWrite(tplPath))
            {
                tpl.Save(tplStream, GetSelectedGame());
            }

            haveUnsavedTplChanges = false;
            UpdateTextureButtons();
            return true;
        }

        private void UpdateTextureDisplay()
        {
            // If no item is selected in the list, don't show the information panel
            if (treeTextures.SelectedNode == null)
            {
                pbTextureImage.Image = null;

                tlpTextureProperties.Visible = false;
                return;
            }

            // Otherwise, extract the TextureReference structure to get the selected texture
            TextureReference textureData = (TextureReference)treeTextures.SelectedNode.Tag;
            TplTexture tex = tpl[textureData.TextureIdx];

            // If the texture is null, then show a "bogus" information panel
            if (tex.IsEmpty)
            {
                pbTextureImage.Image = null;

                tlpTextureProperties.Visible = true;
                lblTextureDimensions.Text = "-";
                lblTextureFormat.Text = "-";
                btnExportTextureLevel.Enabled = false;
                //btnImportTextureLevel.Enabled = false;

                return;
            }

            // If selecting the whole texture, then show data about the first level, otherwise from the selected model
            int effTextureLevel = (textureData.TextureLevel != -1) ? textureData.TextureLevel : 0;

            pbTextureImage.Image = tex.DecodeLevelToBitmap(effTextureLevel);

            tlpTextureProperties.Visible = true;
            lblTextureDimensions.Text = string.Format("{0} x {1}",
                tex.WidthOfLevel(effTextureLevel), tex.HeightOfLevel(effTextureLevel));
            lblTextureFormat.Text = string.Format("{0} ({1})", tex.Format, EnumUtils.GetEnumDescription(tex.Format));
            btnExportTextureLevel.Enabled = true;
            btnImportTextureLevel.Enabled = true;
        }

        private void UpdateTextureTree()
        {
            treeTextures.Nodes.Clear();
            if (tpl != null)
            {
                for (int i = 0; i < tpl.Count; i++)
                {
                    TreeNode textureItem = new TreeNode(string.Format("Texture {0}", i));
                    textureItem.ForeColor = (!tpl[i].IsEmpty) ? Color.DarkGreen : Color.Red;
                    textureItem.Tag = new TextureReference(i, -1);
                    treeTextures.Nodes.Add(textureItem);

                    // Add subitems for the texture levels
                    if (!tpl[i].IsEmpty)
                    {
                        for (int j = 0; j < tpl[i].LevelCount; j++)
                        {
                            TreeNode levelItem = new TreeNode(string.Format("Level {0}", j));
                            levelItem.Tag = new TextureReference(i, j);
                            textureItem.Nodes.Add(levelItem);
                        }
                    }
                }
            }
        }

        private void UpdateTextureButtons()
        {
            tsBtnSaveTpl.Enabled = (tpl != null && haveUnsavedTplChanges);
            tsBtnSaveTplAs.Enabled = (tpl != null);
        }

        private void tsBtnImportObjMtl_Click(object sender, EventArgs e)
        {
            if (!CheckSaveUnsavedChanges())
                return;

            if (ofdLoadObj.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                ImportObjMtl(ofdLoadObj.FileName, false);
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Error loading the OBJ file. " + ex.Message, "Error loading the OBJ file.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
            }
        }

        public void ImportObjMtl(string filename, bool commandLine)
        {
            List<string> modelWarningLog;
            ObjMtlModel model;
            try
            {
                model = new ObjMtlModel(filename, out modelWarningLog);
                if (modelWarningLog.Count != 0)
                {
                    if (!commandLine)
                    {
                        ObjMtlWarningLogDialog warningDlg = new ObjMtlWarningLogDialog(modelWarningLog);
                        if (warningDlg.ShowDialog() != DialogResult.Yes)
                            return;
                    }
                    else
                    {
                        Console.WriteLine("Obj Import Warnings:");
                        foreach(string warning in modelWarningLog)
                        {
                            Console.WriteLine("Import Warning: " + warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Dictionary<Bitmap, int> textureIndexMapping;
            tpl = new Tpl(model, GetSelectedMipmap(), GetNumMipmaps(), out textureIndexMapping);
            gma = new Gma(model, textureIndexMapping);

            // Set TPL / GMA as changed
            haveUnsavedGmaChanges = true;
            haveUnsavedTplChanges = true;

            // Update model list
            UpdateModelTree();
            UpdateModelButtons();
            UpdateModelDisplay();

            // Update material tab
            UpdateMaterialList();
            UpdateMaterialDisplay();

            // Update texture list
            UpdateTextureTree();
            UpdateTextureButtons();
            UpdateTextureDisplay();

            // Update model viewer
            reloadOnNextRedraw = true;
            glControlModel.Invalidate();
        }

        private void tsBtnExportObjMtl_Click(object sender, EventArgs e)
        {
            if (fbdModelExportPath.ShowDialog() == DialogResult.OK)
            {
                // Export OBJ and MTL files
                ObjMtlExporter exporter = new ObjMtlExporter(fbdModelExportPath.SelectedPath);

                // Export textures
                if (tpl != null)
                {
                    for (int i = 0; i < tpl.Count; i++)
                    {
                        exporter.ExportTexture(i, tpl[i]);
                    }
                }

                // Export model
                if (gma != null)
                {
                    exporter.ExportModel(gma);
                }
            }
        }

        private void TextureHasChanged(int idx)
        {
            haveUnsavedTplChanges = true;
            UpdateTextureButtons();
            UpdateTextureDisplay();
            
            glControlModel.MakeCurrent();
            ctx.SetTexture(idx, tpl[idx]);
            glControlModel.Invalidate();
        }

        private void treeModel_AfterCheckState(object sender, TreeViewEventArgs e)
        {
            glControlModel.Invalidate();
        }

        private void treeModel_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateModelDisplay();
            UpdateMaterialList();
            UpdateMaterialDisplay();
        }

        private void SetVisibleModelMeshes(bool showLayer1, bool showLayer2)
        {
            foreach (TreeNode modelItem in treeModel.Nodes)
            {
                if (modelItem.Nodes.Count > 0)
                {
                    // Set model meshes check state
                    foreach (TreeNode meshItem in modelItem.Nodes)
                    {
                        ModelMeshReference meshData = (ModelMeshReference)meshItem.Tag;
                        GcmfMesh mesh = gma[meshData.ModelIdx].ModelObject.Meshes[meshData.MeshIdx];
                        if (mesh.Layer == GcmfMesh.MeshLayer.Layer1)
                        {
                            treeModel.SetCheckState(meshItem, showLayer1 ? CheckState.Checked : CheckState.Unchecked);
                        }
                        else if (mesh.Layer == GcmfMesh.MeshLayer.Layer2)
                        {
                            treeModel.SetCheckState(meshItem, showLayer2 ? CheckState.Checked : CheckState.Unchecked);
                        }
                    }
                }
                else
                {
                    // For the orphan nodes (models with no meshes, mostly GMA null entries),
                    // unselect them unless we're choosing to show both types of layers
                    treeModel.SetCheckState(modelItem, (showLayer1 && showLayer2) ? CheckState.Checked : CheckState.Unchecked);
                }
            }

            // The model will be updated due to the AfterCheckState event on treeModel
        }

        private void btnModelShowAll_Click(object sender, EventArgs e)
        {
            SetVisibleModelMeshes(true, true);
        }

        private void btnModelShowLayer1_Click(object sender, EventArgs e)
        {
            SetVisibleModelMeshes(true, false);
        }

        private void btnModelShowLayer2_Click(object sender, EventArgs e)
        {
            SetVisibleModelMeshes(false, true);
        }

        private void btnModelHideAll_Click(object sender, EventArgs e)
        {
            SetVisibleModelMeshes(false, false);
        }

        private void treeTextures_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateTextureDisplay();
        }

        private void toolStripMenuItemResetViewer_Click(object sender, EventArgs e)
        {
            angleX = 0.0f;
            angleY = 0.0f;
            zoomFactor = 1.0f;
            glControlModel.Invalidate();
        }

        private void toolStripMenuItemShowTextures_Click(object sender, EventArgs e)
        {
            glControlModel.MakeCurrent();

            if (toolStripMenuItemShowTextures.Checked)
                GL.Enable(EnableCap.Texture2D);
            else
                GL.Disable(EnableCap.Texture2D);

            glControlModel.Invalidate();
        }

        private void treeMaterials_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateMaterialDisplay();
        }

        private void ModelViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckSaveUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }

            glControlModel.MakeCurrent();
            UnloadModel();
        }

        private void mipmapDropDownItemClicked(object sender, EventArgs e)
        {
            string text = ((ToolStripMenuItem)sender).Text;
            int mipmapAmt = int.Parse(text);
            numMipmaps = mipmapAmt;

            foreach(ToolStripMenuItem item in mipmapItems)
            {
                if(item.Text == text)
                {
                    item.Checked = true;
                }
                else if (item.Checked)
                {
                    item.Checked = false;
                }
            }
        }

        private void mipmapInterpolationDropDownItemClicked(object sender, EventArgs e)
        {
            string text = ((ToolStripMenuItem)sender).Text;
            GxInterpolationFormat intEnum = EnumUtils.GetValueFromDescription<GxInterpolationFormat>(text);
            intFormat = intEnum;

            foreach (ToolStripMenuItem item in mipMapIntItems)
            {
                if (item.Text == text)
                {
                    item.Checked = true;
                }
                else if (item.Checked)
                {
                    item.Checked = false;
                }
            }
        }

        private void gmaExportTolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            // Select the clicked node
            TreeNode selected = treeModel.SelectedNode;

            if (selected != null)
            {
                string nodeName = selected.Text;

                if (fbdModelExportPath.ShowDialog() == DialogResult.OK)
                {
                    // Export OBJ and MTL files
                    ObjMtlExporter exporter = new ObjMtlExporter(fbdModelExportPath.SelectedPath);


                    // Export model
                    if (gma != null)
                    {
                        List<int> textureIds = exporter.ExportModel(gma, nodeName);

                        if (tpl != null)
                        {
                            for (int i = 0; i < tpl.Count; i++)
                            {
                                if (textureIds.Contains(i))
                                {
                                    exporter.ExportTexture(i, tpl[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void gmaImporttoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select the clicked node
            TreeNode selected = treeModel.SelectedNode;
            gmaImport(selected, false);
        }

        private void gmaImportPreserveFlagstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select the clicked node
            TreeNode selected = treeModel.SelectedNode;
            gmaImport(selected, true);
        }

        private void gmaImport(TreeNode selected, bool preserveFlags)
        {
            if (selected != null)
            {
                string nodeName = selected.Text;

                if (ofdLoadObj.ShowDialog() != DialogResult.OK)
                    return;

                List<string> modelWarningLog;
                ObjMtlModel model;
                try
                {
                    model = new ObjMtlModel(ofdLoadObj.FileName, out modelWarningLog);
                    if (modelWarningLog.Count != 0)
                    {
                        ObjMtlWarningLogDialog warningDlg = new ObjMtlWarningLogDialog(modelWarningLog);
                        if (warningDlg.ShowDialog() != DialogResult.Yes)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading the OBJ file. " + ex.Message, "Error loading the OBJ file.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Dictionary<Bitmap, int> textureIndexMapping;
                List<int> textureIds = gma.GetTextureIds(nodeName);
                tpl.Load(model, GetSelectedMipmap(), GetNumMipmaps(), textureIds, out textureIndexMapping);
                gma.Load(model, textureIndexMapping, nodeName, preserveFlags);

                // Set TPL / GMA as changed
                haveUnsavedGmaChanges = true;
                haveUnsavedTplChanges = true;

                // Update model list
                UpdateModelTree();
                UpdateModelButtons();
                UpdateModelDisplay();

                // Update material tab
                UpdateMaterialList();
                UpdateMaterialDisplay();

                // Update texture list
                UpdateTextureTree();
                UpdateTextureButtons();
                UpdateTextureDisplay();

                // Update model viewer
                reloadOnNextRedraw = true;
                glControlModel.Invalidate();
            }
        }

        private void btnExportTextureLevel_Click(object sender, EventArgs e)
        {
            // Extract the TextureReference structure to get the selected texture
            TextureReference textureData = (TextureReference)treeTextures.SelectedNode.Tag;
            TplTexture tex = tpl[textureData.TextureIdx];

            // If selecting the whole texture, then export data about the first level, otherwise from the selected model
            int effTextureLevel = (textureData.TextureLevel != -1) ? textureData.TextureLevel : 0;

            sfdTextureExportPath.FileName = string.Format("{0}_{1}.png", textureData.TextureIdx, effTextureLevel);
            if (sfdTextureExportPath.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                tex.DecodeLevelToBitmap(effTextureLevel).Save(sfdTextureExportPath.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The texture could not be exported: " + ex.Message,
                    "Error exporting texture.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportTextureLevel_Click(object sender, EventArgs e)
        {
            // Extract the TextureReference structure to get the selected texture
            TextureReference textureData = (TextureReference)treeTextures.SelectedNode.Tag;
            TplTexture tex = tpl[textureData.TextureIdx];

            // Request image filename
            if (ofdTextureImportPath.ShowDialog() != DialogResult.OK)
                return;

            // Load image
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(ofdTextureImportPath.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading image.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (textureData.TextureLevel == -1) // Replacing whole texture
            {
                // Ask the user to select the format to import
                GxTextureFormatPickerDialog formatPickerDlg = new GxTextureFormatPickerDialog(
                    TplTexture.SupportedTextureFormats, tex.Format);
                if (formatPickerDlg.ShowDialog() != DialogResult.OK)
                    return;

                GxTextureFormat newFmt = formatPickerDlg.SelectedFormat;

                // Redefine the entire texture from the bitmap
                tex.DefineTextureFromBitmap(newFmt, GetSelectedMipmap(), GetNumMipmaps(), bmp);

                TextureHasChanged(textureData.TextureIdx);
                UpdateTextureTree();
                treeTextures.SelectedNode = treeTextures.Nodes.Cast<TreeNode>()
                    .Where(tn => ((TextureReference)tn.Tag).TextureIdx == textureData.TextureIdx).First();
            }
            else // Replacing single level
            {
                if (bmp.Width != tex.WidthOfLevel(textureData.TextureLevel) ||
                    bmp.Height != tex.HeightOfLevel(textureData.TextureLevel))
                {
                    MessageBox.Show("The selected image has an invalid size to replace this level.\n" +
                        "If you wish to replace the entire texture, select the node coresponding to the texture.",
                        "Invalid image size", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // Replace just the selected level from the bitmap
                tex.DefineLevelDataFromBitmap(textureData.TextureLevel, GetSelectedMipmap(), bmp);

                TextureHasChanged(textureData.TextureIdx);
            }
        }

        private void editModelFlagstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select the clicked node
            TreeNode selected = treeModel.SelectedNode;

            int index = gma.GetEntryIndex(selected.Text);
            Gcmf model = gma[index].ModelObject;

            using (ModelFlagEditor flagEditor = new ModelFlagEditor(model))
            {
                switch (flagEditor.ShowDialog())
                {
                    case DialogResult.OK:
                        UpdateModelDisplay();
                        break;
                }
            }
        }

        private void editMeshFlagstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select the clicked node
            TreeNode selected = treeModel.SelectedNode;
            TreeNode parent = selected.Parent;
            int meshIndex = selected.Index;
            int modelIndex = gma.GetEntryIndex(parent.Text);
            Gcmf model = gma[modelIndex].ModelObject;
            GcmfMesh mesh = model.Meshes[meshIndex];
 
            using (MeshFlagEditor meshEditor = new MeshFlagEditor(mesh))
            {
                switch (meshEditor.ShowDialog())
                {
                    case DialogResult.OK:
                        UpdateModelDisplay();
                        UpdateModelTree();
                        break;
                }
            }
        }

        private void editMaterialFlagstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select the clicked node
            TreeNode selected = treeMaterials.SelectedNode;
            ModelMaterialReference itemData = (ModelMaterialReference)selected.Tag;
            GcmfMaterial material = gma[itemData.ModelIdx].ModelObject.Materials[itemData.MaterialIdx];

            using (MaterialFlagEditor materialEditor = new MaterialFlagEditor(material))
            {
                switch (materialEditor.ShowDialog())
                {
                    case DialogResult.OK:
                        UpdateMaterialDisplay();
                        break;
                }
            }
        }
    }
}
