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
using System.Text.RegularExpressions;
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

            public ModelMaterialReference(int modelIdx, int materialIdx)
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

        bool deleteUnusedTexturesAuto = false;
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
        ToolStripMenuItem[] mipmapItems = new ToolStripMenuItem[10];

        GxInterpolationFormat intFormat = GxInterpolationFormat.HighQualityBicubic;
        ToolStripMenuItem[] mipMapIntItems;

        /// <summary> Whether or not the current selection can be renamed. </summary>
        bool canRenameCurrentSelection = false;

        /// <summary> Whether or not the current TPL was imported as a headerless TPL.</summary>
        bool currentTplHeaderless = false;

        /// <summary> Whether or not to render values in the UI as hexadecimal. </summary>
        bool hexadecimalNumbers = true;

        public ModelViewer()
        {
            InitializeComponent();
            glControlModel.MouseWheel += glControlModel_MouseWheel;

            // Populate ComboBox values from GxGame enum dynamically.
            tsCmbGame.ComboBox.ValueMember = "Key";
            tsCmbGame.ComboBox.DisplayMember = "Value";
            tsCmbGame.ComboBox.DataSource = new BindingSource(Enum.GetValues(typeof(GxGame)).Cast<GxGame>()
                .Select(g => new { Key = g, Value = EnumUtils.GetEnumDescription(g) }).ToArray(), null);


            // Populate the Menu Strip for the number of mipmaps
            int i;
            for (i = 0; i < mipmapItems.Length - 1; ++i)
            {
                ToolStripMenuItem item = new ToolStripMenuItem("" + (i + 1));
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
            foreach (GxInterpolationFormat intFormatEnum in interpolationTypes)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(EnumUtils.GetEnumDescription(intFormatEnum));
                item.Click += new EventHandler(mipmapInterpolationDropDownItemClicked);
                mipMapIntItems[i] = item;
                if (intFormatEnum == intFormat)
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

        public void SetSelectedGame(GxGame newGame)
        {
            tsCmbGame.ComboBox.SelectedValue = newGame;
        }

        private GxInterpolationFormat GetSelectedMipmap()
        {
            return intFormat;
        }
        public void SetSelectedMipmap(GxInterpolationFormat format)
        {
            intFormat = format;
        }

        private int GetNumMipmaps()
        {
            return numMipmaps;
        }

        public void SetNumMipmaps(int num)
        {
            numMipmaps = num;
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
                        if (modelObjects != null)
                        {
                            // Whole item is checked -> Call the display list corresponding to the entire model
                            ctx.CallDisplayList(modelObjects[modelRef.ModelIdx].Value);
                        }
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

            try
            {
                LoadGmaFile(ofdLoadGma.FileName);
            }
            catch (Exception ex)
            {
                if (tpl != null)
                {
                    DialogResult unloadChoice = MessageBox.Show("The currently loaded TPL does not appear to be compatible with the selected GMA file.\nWould you like to unload the current TPL?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (unloadChoice == DialogResult.Yes)
                    {
                        LoadTplFile(null);
                        LoadGmaFile(ofdLoadGma.FileName);
                    }
                }

                else
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void LoadGmaFile(string newGmaPath)
        {
            Exception exception = null;
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
                    gma = null;
                    gmaPath = null;
                    exception = ex;
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

            // Update texture tab
            UpdateTextureDisplay();

            // Update model viewer
            reloadOnNextRedraw = true;
            glControlModel.Invalidate();

            // Throw delayed until end to keep previous functionality (clear GMA on error)
            if (exception != null)
            {
                throw exception;
            }

            // Update context menu
            gmaExportTolStripMenuItem.Enabled = true;
            gmaImporttoolStripMenuItem.Enabled = true;
            importPreserveFLagsToolStripMenuItem.Enabled = true;
            editFlagsToolStripMenuItem.Enabled = true;
            renameToolStripMenuItem.Enabled = true;
            removeToolStripMenuItem.Enabled = true;
            removeToolStripMenuItem1.Enabled = true;
            removeUnusedToolStripMenuItem.Enabled = true;

            if (tpl != null) UpdateTexturesUsedBy();
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

            using (Stream gmaStream = File.Create(gmaPath))
            {
                gma.Save(gmaStream, GetSelectedGame());
            }

            haveUnsavedGmaChanges = false;
            UpdateModelButtons();
            return true;
        }

        public bool SaveGmaFile(string filename)
        {
            // Unlike the UI version, this always sets a new underlying GMA
            gmaPath = filename;

            using (Stream gmaStream = File.Create(gmaPath))
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
            bool gmaNotNull = (gma != null);
            tsBtnSaveGma.Enabled = (gmaNotNull && haveUnsavedGmaChanges);
            tsBtnSaveGmaAs.Enabled = (gmaNotNull);

            tsBtnExportObjMtl.Enabled = (gmaNotNull);

            btnModelShowAll.Enabled = (gmaNotNull);
            btnModelShowLayer1.Enabled = (gmaNotNull);
            btnModelShowLayer2.Enabled = (gmaNotNull);
            btnModelHideAll.Enabled = (gmaNotNull);
        }

        private void UpdateModelDisplay()
        {
            // If no item is selected in the list, hide the display completely
            if (treeModel.SelectedNodes.Count == 0)
            {
                tlpModelDisplay.Visible = false;
                tlpMeshDisplay.Visible = false;
                return;
            }

            // Otherwise, extract the ModelTreeItem structure to get the selected model/mesh
            ModelMeshReference modelMeshReference = (ModelMeshReference)treeModel.SelectedNodes[0].Tag;
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

                String indexFormat = (hexadecimalNumbers) ? "{0:X}" : "{0}";
                lblMeshRenderFlags.Text = string.Format("0x{0:X8} ({1})", (uint)mesh.RenderFlags, EnumUtils.GetEnumFlagsString(mesh.RenderFlags));
                lblMeshUnk4.Text = string.Format("0x{0:X8}", mesh.Unk4);
                lblMeshUnk8.Text = string.Format("0x{0:X8}", mesh.Unk8);
                lblMeshUnkC.Text = string.Format("0x{0:X8}", mesh.UnkC);
                lblMeshUnk10.Text = string.Format("0x{0:X4}", mesh.Unk10);
                lblMeshUnk12.Text = string.Format("0x{0:X2}", Convert.ToByte(((mesh.PrimaryMaterialIdx != ushort.MaxValue) ? 1 : 0) +
                                     ((mesh.SecondaryMaterialIdx != ushort.MaxValue) ? 1 : 0) +
                                     ((mesh.TertiaryMaterialIdx != ushort.MaxValue) ? 1 : 0)));
                lblMeshUnk14.Text = string.Format("0x{0:X4}", mesh.Unk14);
                lblMeshPrimaryMaterialIdx.Text = (mesh.PrimaryMaterialIdx == 0xFFFF) ? "None" : string.Format(indexFormat, mesh.PrimaryMaterialIdx);
                lblMeshSecondaryMaterialIdx.Text = (mesh.SecondaryMaterialIdx == 0xFFFF) ? "None" : string.Format(indexFormat, mesh.SecondaryMaterialIdx);
                lblMeshTertiaryMaterialIdx.Text = (mesh.TertiaryMaterialIdx == 0xFFFF) ? "None" : string.Format(indexFormat, mesh.TertiaryMaterialIdx);
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
            if (treeModel.SelectedNodes.Count == 0)
                return -1;

            // Otherwise, extract the model/mesh reference structure and get the model index from there
            ModelMeshReference itemData = (ModelMeshReference)treeModel.SelectedNodes[0].Tag;
            return ((ModelMeshReference)treeModel.SelectedNodes[0].Tag).ModelIdx;
        }

        private GcmfMaterial GetSelectedMaterial()
        {
            // If no item is selected in the list, return nullptr
            if (treeMaterials.SelectedNodes.Count == 0)
                return null;

            // Otherwise, extract the ModelMaterialReference structure to get the selected model/mesh
            ModelMaterialReference itemData = (ModelMaterialReference)treeMaterials.SelectedNodes[0].Tag;
            return gma[itemData.ModelIdx].ModelObject.Materials[itemData.MaterialIdx];
        }

        private void UpdateMaterialList()
        {
            treeMaterials.Clear();
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
                String indexFormat = (hexadecimalNumbers) ? "Material {0:X}" : "Material {0}";
                TreeNode materialItem = new TreeNode(string.Format(indexFormat, i));
                materialItem.Tag = new ModelMaterialReference(modelIdx, i);
                materialItem.ContextMenuStrip = materialMenuStrip;
                treeMaterials.Nodes.Add(materialItem);
            }
        }

        private void UpdateMaterialDisplay()
        {

            GcmfMaterial material = GetSelectedMaterial();

            // If no model is selected, do not allow the definition of a new material
            defineNewToolStripMenuItem.Enabled = (gma != null && GetSelectedModelIdx() != -1);
            defineNewFromTextureToolStripMenuItem.Enabled = (gma != null && tpl != null && GetSelectedModelIdx() != -1);

            // If no model or material is selected, make the fields blank.
            if (material == null)
            {
                pbMaterialTextureImage.Image = null;
                tlpMaterialProperties.Visible = false;
                return;
            }

            String indexFormat = (hexadecimalNumbers) ? "{0:X}" : "{0}";
            tlpMaterialProperties.Visible = true;
            lblMaterialFlags.Text = string.Format("0x{0:X8}", material.Flags);
            lblMaterialTextureIndex.Text = string.Format("{0:X}", material.TextureIdx);
            lblMaterialUnk6.Text = string.Format(indexFormat, material.Unk6);
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
            bool pressingShift = (Control.ModifierKeys == Keys.Shift);
            if (!CheckSaveUnsavedChanges())
                return;

            // Suggest the name associated with the loaded .GMA file if one is loaded
            if (gmaPath != null && gmaPath.EndsWith(".gma"))
                ofdLoadTpl.FileName = gmaPath.Substring(0, gmaPath.Length - 3) + "tpl";

            // Ask the user for a TPL file
            if (ofdLoadTpl.ShowDialog() != DialogResult.OK)
            {
                UpdateTextureButtons();
                return;
            }

            try
            {
                LoadTplFile(ofdLoadTpl.FileName, pressingShift);
                tsBtnLoadTpl.Text = "Load TPL...";
            }
            catch (Exception ex)
            {
                if (gma != null)
                {
                    DialogResult unloadChoice = MessageBox.Show("The currently loaded GMA does not appear to be compatible with the selected TPL file.\nWould you like to unload the current GMA?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (unloadChoice == DialogResult.Yes)
                    {
                        LoadGmaFile(null);
                        LoadTplFile(ofdLoadTpl.FileName);
                    }
                }

                else
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }

        public void LoadTplFile(string newTplPath, bool pressingShift = false)
        {
            Exception exception = null;
            // Try to load the TPL file
            if (newTplPath != null)
            {
                try
                {
                    using (Stream tplStream = File.OpenRead(newTplPath))
                    {
                        // Checks if the TPL does not have a standard header
                        // This check does not always work, specifically if the TPL starts with a transparent image
                        // So, we allow the pressing of Shift to force this to occur
                        if ((tplStream.ReadByte() != 0 && GetSelectedGame() == GxGame.SuperMonkeyBall) || pressingShift)
                        {
                            // Gets the file size, to get the number of textures in the file
                            int fileSize = (int)new System.IO.FileInfo(newTplPath).Length;
                            AddTextureHeader addTextureHeader = new AddTextureHeader(TplTexture.SupportedTextureFormats, GxTextureFormat.RGB5A3, newTplPath, fileSize);
                            addTextureHeader.ShowDialog();
                            if (addTextureHeader.DialogResult == DialogResult.OK)
                            {
                                GeneratedTextureHeader? newHeader = addTextureHeader.getTextureHeader();
                                tplStream.Position = 0;
                                tpl = new Tpl(tplStream, GetSelectedGame(), newHeader);
                                currentTplHeaderless = true;
                                numMipmaps = 1;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            numMipmaps = 255;

                            tplStream.Position = 0;
                            tpl = new Tpl(tplStream, GetSelectedGame());
                            currentTplHeaderless = false;
                        }
                        tplPath = newTplPath;
                    }
                }
                catch (Exception ex)
                {
                    tpl = null;
                    tplPath = null;
                    exception = ex;
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

            // Update material list
            UpdateMaterialDisplay();

            // Throw delayed until end to keep previous functionality (clear TPL on error)           
            if (exception != null)
            {
                throw exception;
            }

            // Updates list of models used by which textures
            UpdateTexturesUsedBy();
        }

        private void tsBtnSaveTpl_Click(object sender, EventArgs e)
        {
            SaveTplFile(currentTplHeaderless || (Control.ModifierKeys == Keys.Shift));
            UpdateTextureButtons();
        }

        private void tsBtnSaveTplAs_Click(object sender, EventArgs e)
        {
            bool shiftPressed = (Control.ModifierKeys == Keys.Shift);
            if (sfdSaveTpl.ShowDialog() != DialogResult.OK)
            {
                UpdateTextureButtons();
                return;
            }
            tplPath = sfdSaveTpl.FileName;
            SaveTplFile(currentTplHeaderless || shiftPressed);
            UpdateTextureButtons();
        }

        private void tplButtonTextHeaderless()
        {
            tsBtnLoadTpl.Text = "Load TPL...";
            if (currentTplHeaderless)
            {
                tsBtnSaveTpl.Text = "Save Headerless TPL...";
                tsBtnSaveTplAs.Text = "Save Headerless TPL As...";
            }
            else
            {
                tsBtnSaveTpl.Text = "Save TPL...";
                tsBtnSaveTplAs.Text = "Save TPL As...";
            }
        }

        private bool SaveTplFile(bool noHeader = false)
        {
            // If there isn't currently any path set (e.g. we've just imported a model),
            // we have to request one to the user
            if (tplPath == null)
            {
                if (sfdSaveTpl.ShowDialog() != DialogResult.OK)
                {
                    UpdateTextureButtons();
                    return false;
                }
                tplPath = sfdSaveTpl.FileName;
            }

            using (Stream tplStream = File.Create(tplPath))
            {
                tpl.Save(tplStream, GetSelectedGame(), noHeader);
            }

            haveUnsavedTplChanges = false;
            UpdateTextureButtons();
            return true;
        }

        public bool SaveTplFile(string filename)
        {
            // Unlike the UI version, this always sets a new underlying TPL
            tplPath = filename;

            using (Stream tplStream = File.Create(tplPath))
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
                lblTextureUsedBy.Text = "-";
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
            lblTextureUsedBy.Text = string.Join(", ", tex.usedByModels.Select(x => x.Name));
            if (tex.usedByModels == null || tex.usedByModels.Count == 0)
            {
                lblTextureUsedBy.Text = "Unused";
            }
            if (gma == null)
            {
                lblTextureUsedBy.Text = "N/A";
            }
            toolTipTextureUsedBy.SetToolTip(lblTextureUsedBy, lblTextureUsedBy.Text);
            btnExportTextureLevel.Enabled = true;
            btnImportTextureLevel.Enabled = true;
        }

        private void UpdateTextureTree()
        {
            // Do not allow new definition of textures if no TPL is loaded
            bool tplNotNull = (tpl != null);
            bool gmaNotNull = (gma != null);

            defineNewToolStripMenuItem1.Enabled = (tplNotNull);
            removeToolStripMenuItem1.Enabled = (tplNotNull) && (gmaNotNull);
            removeUnusedToolStripMenuItem.Enabled = (tplNotNull) && (gmaNotNull);
            deletenoMaterialAdjustmentToolStripMenuItem.Enabled = (tplNotNull);

            treeTextures.Nodes.Clear();

            String indexFormat = (hexadecimalNumbers) ? "Texture {0:X}" : "Texture {0}";
            if (tpl != null)
            {
                for (int i = 0; i < tpl.Count; i++)
                {
                    TreeNode textureItem = new TreeNode(string.Format(indexFormat, i));
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
            tplButtonTextHeaderless();
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
                        foreach (string warning in modelWarningLog)
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
            UpdateTexturesUsedBy();

            // Update model viewer
            reloadOnNextRedraw = true;
            glControlModel.Invalidate();
        }

        private void tsBtnExportObjMtl_Click(object sender, EventArgs e)
        {
            if (sfdModelExportPath.ShowDialog() == DialogResult.OK)
            {
                string directory = Path.GetDirectoryName(sfdModelExportPath.FileName);
                string pathWithoutExtension = Path.GetFileNameWithoutExtension(sfdModelExportPath.FileName);
                // Export OBJ and MTL files
                ObjMtlExporter exporter = new ObjMtlExporter(directory, pathWithoutExtension);

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

        public void ExportObjMtl(string objFilename)
        {
            string directory = Path.GetDirectoryName(objFilename);
            string pathWithoutExtension = Path.GetFileNameWithoutExtension(objFilename);
            // Export OBJ and MTL files
            ObjMtlExporter exporter = new ObjMtlExporter(directory, pathWithoutExtension);

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

            foreach (ToolStripMenuItem item in mipmapItems)
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
            TreeNode selected = treeModel.SelectedNodes[0];

            if (selected != null)
            {
                string nodeName = selected.Text;

                if (sfdModelExportPath.ShowDialog() == DialogResult.OK)
                {
                    string directory = Path.GetDirectoryName(sfdModelExportPath.FileName);
                    string pathWithoutExtension = Path.GetFileNameWithoutExtension(sfdModelExportPath.FileName);
                    // Export OBJ and MTL files
                    ObjMtlExporter exporter = new ObjMtlExporter(directory, pathWithoutExtension);


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
            TreeNode selected = treeModel.SelectedNodes[0];
            gmaImport(selected, false);
        }

        private void gmaImportPreserveFlagstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select the clicked node
            TreeNode selected = treeModel.SelectedNodes[0];
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

            // If selecting the whole texture, then export data about the first level, otherwise loop and extract all levels
            int effTextureLevel = (textureData.TextureLevel != -1) ? textureData.TextureLevel : 0;

            sfdTextureExportPath.FileName = string.Format("{0}_{1}.png", textureData.TextureIdx, effTextureLevel);

            if (sfdTextureExportPath.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                if (textureData.TextureLevel != -1)
                {
                    tex.DecodeLevelToBitmap(effTextureLevel).Save(sfdTextureExportPath.FileName);
                }
                else
                {
                    for (effTextureLevel = 0; effTextureLevel < tpl[textureData.TextureIdx].LevelCount; effTextureLevel++)
                    {
                        sfdTextureExportPath.FileName = Regex.Replace(sfdTextureExportPath.FileName, @"\d{1,}(?=\..{3,4}$)", effTextureLevel.ToString());
                        tex.DecodeLevelToBitmap(effTextureLevel).Save(sfdTextureExportPath.FileName);
                    }

                }
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
                try
                {
                    // Redefine the entire texture from the bitmap
                    tex.DefineTextureFromBitmap(newFmt, GetSelectedMipmap(), GetNumMipmaps(), bmp, ofdTextureImportPath.FileName);
                    TextureHasChanged(textureData.TextureIdx);
                    UpdateTextureTree();
                    treeTextures.SelectedNode = treeTextures.Nodes.Cast<TreeNode>()
                    .Where(tn => ((TextureReference)tn.Tag).TextureIdx == textureData.TextureIdx).First();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while importing the texture(s).\n" +
                                    "If you are importing multiple mipmap levels, ensure\n",
                                    "that all of the mipmaps are the correct size.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

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
            // Grab selected nodes
            List<TreeNode> selectedNodes = treeModel.SelectedNodes;
            List<Gcmf> models = new List<Gcmf>(selectedNodes.Count);
            foreach (TreeNode node in selectedNodes)
            {
                // Make sure these are all models, not meshes
                if (node.Parent == null)
                {
                    int index = gma.GetEntryIndex(node.Text);
                    models.Add(gma[index].ModelObject);
                }
            }

            using (ModelFlagEditor flagEditor = new ModelFlagEditor(models))
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
            // Grab selected nodes
            List<TreeNode> selectedNodes = treeModel.SelectedNodes;
            List<GcmfMesh> meshes = new List<GcmfMesh>(selectedNodes.Count);
            foreach (TreeNode node in selectedNodes)
            {
                // Make sure these are all meshes, not models
                if (node.Parent != null)
                {
                    TreeNode parent = node.Parent;
                    int meshIndex = node.Index;
                    int modelIndex = gma.GetEntryIndex(parent.Text);
                    Gcmf model = gma[modelIndex].ModelObject;
                    meshes.Add(model.Meshes[meshIndex]);
                }
            }

            using (MeshFlagEditor meshEditor = new MeshFlagEditor(meshes))
            {
                switch (meshEditor.ShowDialog())
                {
                    case DialogResult.OK:
                        UpdateModelDisplay();
                        UpdateModelTree();
                        break;
                    case DialogResult.Yes:
                        UpdateModelDisplay();
                        UpdateModelTree();
                        reloadOnNextRedraw = true;
                        glControlModel.Invalidate();
                        break;
                }
            }
        }

        private void removeMaterial()
        {
            int matIndex = treeMaterials.SelectedNode.Index;
            foreach (GcmfMesh meshCheck in gma[GetSelectedModelIdx()].ModelObject.Meshes)
            {
                if (meshCheck.PrimaryMaterialIdx == matIndex || meshCheck.SecondaryMaterialIdx == matIndex || meshCheck.TertiaryMaterialIdx == matIndex)
                {
                    DialogResult removeResult = MessageBox.Show("This material is currently assigned to a mesh in the model " + gma[GetSelectedModelIdx()].Name + ".\nMaterials must be unassigned before removal. Would you like to unassign this material from the associated mesh?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (removeResult == DialogResult.Yes)
                    {
                        if (meshCheck.PrimaryMaterialIdx == matIndex)
                        {
                            meshCheck.PrimaryMaterialIdx = 0xFFFF;
                        }
                        if (meshCheck.SecondaryMaterialIdx == matIndex)
                        {
                            meshCheck.SecondaryMaterialIdx = 0xFFFF;
                        }
                        if (meshCheck.TertiaryMaterialIdx == matIndex)
                        {
                            meshCheck.TertiaryMaterialIdx = 0xFFFF;
                        }
                        UpdateModelDisplay();
                    }
                    else
                    {
                        return;
                    }
                }

                else
                {
                    if (meshCheck.PrimaryMaterialIdx > matIndex && meshCheck.PrimaryMaterialIdx != 0xFFFF)
                    {
                        meshCheck.PrimaryMaterialIdx--;
                    }
                    if (meshCheck.SecondaryMaterialIdx > matIndex && meshCheck.SecondaryMaterialIdx != 0xFFFF)
                    {
                        meshCheck.SecondaryMaterialIdx--;
                    }
                    if (meshCheck.TertiaryMaterialIdx > matIndex && meshCheck.TertiaryMaterialIdx != 0xFFFF)
                    {
                        meshCheck.TertiaryMaterialIdx--;
                    }
                }

            }
            gma[GetSelectedModelIdx()].ModelObject.Materials.RemoveAt(matIndex);

            UpdateMaterialList();
            if (gma[GetSelectedModelIdx()].ModelObject.Materials.Count > 1)
            {
                treeMaterials.SelectedNode = treeMaterials.Nodes[gma[GetSelectedModelIdx()].ModelObject.Materials.Count - 1];
            }
            UpdateMaterialDisplay();
            UpdateTexturesUsedBy();
            reloadOnNextRedraw = true;
        }

        private void defineNewMaterial(int inputTextureIdx = 0)
        {
            GcmfMaterial newMaterial = new GcmfMaterial();
            newMaterial.TextureIdx = (ushort)(inputTextureIdx);
            gma[GetSelectedModelIdx()].ModelObject.Materials.Add(newMaterial);
            List<GcmfMaterial> materialAsList = new List<GcmfMaterial>();
            materialAsList.Add(newMaterial);

            using (MaterialFlagEditor materialEditor = new MaterialFlagEditor(materialAsList))
            {
                switch (materialEditor.ShowDialog())

                {
                    case DialogResult.OK:
                        UpdateMaterialList();
                        treeMaterials.SelectedNode = treeMaterials.Nodes[gma[GetSelectedModelIdx()].ModelObject.Materials.Count - 1];
                        UpdateMaterialDisplay();
                        UpdateTexturesUsedBy();
                        reloadOnNextRedraw = true;
                        break;
                }
            }
        }
        private void defineNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            defineNewMaterial();
        }

        private void materialMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Do not allow for the editing of flags if no material is selected
            editFlagsToolStripMenuItem2.Enabled = (treeMaterials.SelectedNodes.Count != 0);
            deleteToolStripMenuItem.Enabled = (treeMaterials.SelectedNodes.Count != 0);
        }

        private int defineNewTextureFromBitmap(bool allowMultiImport = false)
        {
            Bitmap bmp;
            if (allowMultiImport)
            {
                ofdTextureImportPath.Multiselect = true;
            }

            if (ofdTextureImportPath.ShowDialog() != DialogResult.OK) return -1;

            // Ask the user to select the format to import
            GxTextureFormatPickerDialog formatPickerDlg = new GxTextureFormatPickerDialog(TplTexture.SupportedTextureFormats, GxTextureFormat.CMPR);
            if (formatPickerDlg.ShowDialog() != DialogResult.OK)
            {
                return -1;
            }

            GxTextureFormat newFmt = formatPickerDlg.SelectedFormat;

            foreach (String newTexturePath in ofdTextureImportPath.FileNames)
            {
                try
                {
                    bmp = new Bitmap(newTexturePath);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error loading image.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }

                TplTexture newTexture = new TplTexture(newFmt, intFormat, numMipmaps, bmp);
                tpl.Add(newTexture);
                int newId = tpl.Count - 1;
                try
                {
                    // Define the entire texture from the bitmap
                    newTexture.DefineTextureFromBitmap(newFmt, GetSelectedMipmap(), GetNumMipmaps(), bmp, ofdTextureImportPath.FileName);
                    TextureHasChanged(newId);
                    UpdateTextureTree();
                    treeTextures.SelectedNode = treeTextures.Nodes[newId];
                    //treeTextures.SelectedNode = treeTextures.Nodes.Cast<TreeNode>()
                    //.Where(tn => ((TextureReference)tn.Tag).TextureIdx == newId).First();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while importing the texture(s).\nIf you are importing multiple mipmap levels, ensure\nthat all of the mipmaps are of the correct size.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }

                if (!allowMultiImport) return newId;
            }
            ofdTextureImportPath.Multiselect = false;
            return -1;
        }

        private void defineNewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            defineNewTextureFromBitmap(true);
        }

        private void defineNewFromTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (defineNewTextureFromBitmap() != -1)
            {
                defineNewMaterial();
            }
        }

        private void treeModel_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                gma[e.Node.Index].Name = e.Label;
                canRenameCurrentSelection = false;
                return;
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            canRenameCurrentSelection = true;
            treeModel.SelectedNode.BeginEdit();
        }

        private void treeModel_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeModel_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode destinationNode = treeModel.GetNodeAt(treeModel.PointToClient(new Point(e.X, e.Y)));
            TreeNode sourceNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (sourceNode.Parent == null && destinationNode.Parent == null)
            {
                GmaEntry entryToBeMoved = new GmaEntry(gma[sourceNode.Index].Name, gma[sourceNode.Index].ModelObject);
                gma.RemoveAt(sourceNode.Index);
                gma.Insert(destinationNode.Index, entryToBeMoved);
                UpdateModelTree();
            }

            else
            {
                e.Effect = DragDropEffects.None;
                MessageBox.Show("Re-ordering individual meshes or moving meshes between models is currently not supported.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void treeModel_DragOver(object sender, DragEventArgs e)
        {
            treeModel.SelectedNode = treeModel.GetNodeAt(treeModel.PointToClient(new Point(e.X, e.Y)));
        }

        private void treeModel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }


        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeModel.SelectedNode != null)
            {
                gma.RemoveAt(treeModel.SelectedNode.Index);
                treeModel.SelectedNodes.Clear();
                UpdateModelDisplay();
                UpdateModelButtons();
                UpdateModelTree();
                if (deleteUnusedTexturesAuto)
                {
                    DeleteUnusedTextures();
                }
                reloadOnNextRedraw = true;
            }
            else
            {
                MessageBox.Show("No model selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void importGMATPLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ofdLoadGma.ShowDialog() != DialogResult.OK)
                return;
            if (ofdLoadTpl.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                using (Stream gmaStream = File.OpenRead(ofdLoadGma.FileName))
                {
                    Gma importedGma = new Gma(gmaStream, GetSelectedGame());
                    foreach (GmaEntry newGmaEntry in importedGma)
                    {
                        foreach (GcmfMaterial newGcmfMat in newGmaEntry.ModelObject.Materials)
                        {
                            // Accounts for the offset of importing new textures to the existing TPL
                            newGcmfMat.TextureIdx += (ushort)tpl.Count;
                        }
                        gma.Add(newGmaEntry);
                    }
                }

                using (Stream tplStream = File.OpenRead(ofdLoadTpl.FileName))
                {
                    Tpl importedTpl = new Tpl(tplStream, GetSelectedGame());
                    foreach (TplTexture newTplTexture in importedTpl)
                    {
                        tpl.Add(newTplTexture);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing a new GMA/TPL.", "Ërror", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateModelButtons();
            UpdateModelTree();
            UpdateModelDisplay();
            UpdateTextureTree();
            UpdateTexturesUsedBy();
            UpdateMaterialDisplay();
            UpdateMaterialList();

            haveUnsavedGmaChanges = true;
            haveUnsavedTplChanges = true;
            reloadOnNextRedraw = true;
            glControlModel.Invalidate();
        }

        private void exportAsGMATPLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdSaveGma.ShowDialog() != DialogResult.OK)
                return;
            if (sfdSaveTpl.ShowDialog() != DialogResult.OK)
                return;

            Gma gmaToBeSaved = new Gma();
            Tpl tplToBeSaved = new Tpl();
            List<TreeNode> selectedNodes = treeModel.SelectedNodes;
            List<int> textureIds = new List<int>();
            try
            {
                foreach (TreeNode node in selectedNodes)
                {
                    gmaToBeSaved.Add(gma[node.Index]);

                    foreach (GcmfMaterial materialToBeSaved in gma[node.Index].ModelObject.Materials)
                    {
                        textureIds.Add(materialToBeSaved.TextureIdx);
                        // Resets the index of the saved materials in accorance with their texture ID
                        materialToBeSaved.TextureIdx = (ushort)(textureIds.Count() - 1);
                    }
                }

                foreach (int textureIdToBeAdded in textureIds)
                {
                    tplToBeSaved.Add(tpl[textureIdToBeAdded]);
                }

                using (Stream gmaStream = File.Create(sfdSaveGma.FileName))
                {
                    gmaToBeSaved.Save(gmaStream, GetSelectedGame());
                }

                using (Stream tplStream = File.Create(sfdSaveTpl.FileName))
                {
                    tplToBeSaved.Save(tplStream, GetSelectedGame());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting as a GMA/TPL.", "Ërror", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeTextures.SelectedNode != null)
            {
                DeleteTextureAt(treeTextures.SelectedNode.Index);
            }
            else
            {
                MessageBox.Show("No texture selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void editMaterialFlagstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Grab selected nodes
            List<TreeNode> selectedNodes = treeMaterials.SelectedNodes;
            List<GcmfMaterial> materials = new List<GcmfMaterial>(selectedNodes.Count);
            foreach (TreeNode node in selectedNodes)
            {
                ModelMaterialReference itemData = (ModelMaterialReference)node.Tag;
                materials.Add(gma[itemData.ModelIdx].ModelObject.Materials[itemData.MaterialIdx]);
            }
            using (MaterialFlagEditor materialEditor = new MaterialFlagEditor(materials))
            {
                switch (materialEditor.ShowDialog())
                {
                    case DialogResult.OK:
                        UpdateMaterialDisplay();
                        UpdateTexturesUsedBy();
                        haveUnsavedGmaChanges = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the mipmap level of all texture to the specified value.
        /// Only currently works when shrinking the number of mipmaps.
        /// </summary>
        /// <param name="level">Number of mipmaps to make all textures</param>
        public void setAllMipmaps(int level)
        {
            if (level < 0)
            {
                throw new InvalidOperationException("Only non-negative mipmap values are allowed.");
            }
            if (tpl != null)
            {
                foreach (TplTexture texture in tpl)
                {
                    texture.LevelCount = level;
                }
            }
            else
            {
                throw new InvalidOperationException("A TPL must be loaded to call this method.");
            }
        }

        private void removeUnusedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteUnusedTextures();
            UpdateTextureTree();
            UpdateTextureButtons();
            UpdateTextureDisplay();
            haveUnsavedTplChanges = true;
        }

        /// <summary>
        /// Determines which models use a texture for all textures in the TPL.
        /// </summary>
        public void UpdateTexturesUsedBy()
        {
            if ((gma != null && tpl != null) && (gma.Count > 0 && tpl.Count > 0))
            {
                // Clears list of models used by textures so it is updated accurately when textures are removed
                foreach (TplTexture tex in tpl)
                {
                    tex.usedByModels.Clear();
                }
                foreach (GmaEntry entry in gma)
                {
                    if (entry != null)
                    {
                        foreach (GcmfMaterial material in entry.ModelObject.Materials)
                        {

                            if (tpl[material.TextureIdx].usedByModels == null || !(tpl[material.TextureIdx].usedByModels.Contains(entry)))
                            {
                                Console.WriteLine("Added model " + entry.Name + " to textureUsed list for texture " + material.TextureIdx);
                                tpl[material.TextureIdx].usedByModels.Add(entry);
                            }
                        }
                    }
                }
            }
        }

        private void deletenoMaterialAdjustmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteTextureAt(treeTextures.SelectedNode.Index, false);
        }

        private void deleteTextureLeftUnusedOnModelDeleteToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            deleteUnusedTexturesAuto = deleteTextureLeftUnusedOnModelDeleteToolStripMenuItem.Checked;
        }

        private void treeModel_MouseDown(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.Shift)
            {
                treeModel.SelectedNode = treeModel.GetNodeAt(e.X, e.Y);
            }
        }

        private void treeMaterials_MouseDown(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.Shift)
            {
                treeMaterials.SelectedNode = treeMaterials.GetNodeAt(e.X, e.Y);
            }
        }

        private void treeTextures_MouseDown(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.Shift)
            {
                treeModel.SelectedNode = treeModel.GetNodeAt(e.X, e.Y);
            }
        }

        private void treeModel_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (!canRenameCurrentSelection)
            {
                e.CancelEdit = true;
            }
        }

        private void reloadViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reloadOnNextRedraw = true;
            angleX = 0.0f;
            angleY = 0.0f;
            zoomFactor = 1.0f;
            glControlModel.Invalidate();
        }

        private void ModelViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                tsBtnLoadTpl.Text = "Load Headerless TPL...";
                if (!currentTplHeaderless)
                {
                    tsBtnSaveTpl.Text = "Save Headerless TPL...";
                    tsBtnSaveTplAs.Text = "Save Headerless TPL As...";
                }
                else
                {
                    tsBtnSaveTpl.Text = "Save TPL...";
                    tsBtnSaveTplAs.Text = "Save TPL As...";
                }
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (treeModel.SelectedNodes.Count != 0)
                {
                    try
                    {
                        List<List<Object>> toCopy = new List<List<object>>();

                        foreach (TreeNode selectedNode in treeModel.SelectedNodes)
                        {
                            ModelMeshReference currentReference = (ModelMeshReference)selectedNode.Tag;

                            toCopy.Add(gma[currentReference.ModelIdx].ModelObject.Meshes[currentReference.MeshIdx].getFlagList());
                        }

                        DataObject clipboardData = new DataObject();
                        clipboardData.SetData("List", toCopy);
                        Clipboard.SetDataObject(clipboardData);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Copy failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                if (treeModel.SelectedNodes.Count != 0)
                {
                    try
                    {
                        ModelMeshReference currentReference = (ModelMeshReference)treeModel.SelectedNodes[0].Tag;

                        List<List<object>> toPaste = (List<List<object>>)(Clipboard.GetDataObject().GetData("List"));
                        for (int currentMesh = 0; currentMesh < toPaste.Count; currentMesh++)
                        {
                            gma[currentReference.ModelIdx].ModelObject.Meshes[currentReference.MeshIdx + currentMesh].setFlagList(toPaste[currentMesh]);
                        }

                        UpdateModelDisplay();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Paste failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ModelViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                tsBtnLoadTpl.Text = "Load TPL...";
                if (currentTplHeaderless)
                {
                    tsBtnSaveTpl.Text = "Save Headerless TPL...";
                    tsBtnSaveTplAs.Text = "Save Headerless TPL As...";
                }
                else
                {
                    tsBtnSaveTpl.Text = "Save TPL...";
                    tsBtnSaveTplAs.Text = "Save TPL As...";
                }
            }
        }

        private void translateModelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (TranslateMesh translateMesh = new TranslateMesh())
            {
                if (treeModel.SelectedNodes.Count == 1)
                {
                    ModelMeshReference selectedReference = (ModelMeshReference)treeModel.SelectedNodes[0].Tag;
                    translateMesh.setInitial(gma[selectedReference.ModelIdx].ModelObject.BoundingSphereCenter);
                }

                switch (translateMesh.ShowDialog())
                {
                    case DialogResult.OK:
                        Vector3 translation = translateMesh.translation;
                        foreach (TreeNode selected in treeModel.SelectedNodes)
                        {
                            ModelMeshReference currentReference = (ModelMeshReference)selected.Tag;
                            foreach (GcmfMesh mesh in gma[currentReference.ModelIdx].ModelObject.Meshes)
                            {
                                foreach (GcmfTriangleStrip strip1ccw in mesh.Obj1StripsCcw)
                                {
                                    foreach (GcmfVertex tri1ccw in strip1ccw)
                                    {
                                        tri1ccw.Position += translation;
                                    }
                                }
                                foreach (GcmfTriangleStrip strip1cw in mesh.Obj1StripsCw)
                                {
                                    foreach (GcmfVertex tri1cw in strip1cw)
                                    {
                                        tri1cw.Position += translation;
                                    }
                                }
                                foreach (GcmfTriangleStrip strip2ccw in mesh.Obj2StripsCcw)
                                {
                                    foreach (GcmfVertex tri2ccw in strip2ccw)
                                    {
                                        tri2ccw.Position += translation;
                                    }
                                }
                                foreach (GcmfTriangleStrip strip2cw in mesh.Obj2StripsCw)
                                {
                                    foreach (GcmfVertex tri2cw in strip2cw)
                                    {
                                        tri2cw.Position += translation;
                                    }
                                }
                                mesh.BoundingSphereCenter += translation;
                            }
                            gma[currentReference.ModelIdx].ModelObject.BoundingSphereCenter += translation;
                        }

                        reloadOnNextRedraw = true;
                        glControlModel.Invalidate();
                        UpdateModelDisplay();

                        break;
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeMaterial();
        }

        private void duplicateModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // stupid hacky workaround because C# doesn't let you easily clone objects
            using (Stream memoryCopy = new MemoryStream()) {
                Gma tempCopyGma = new Gma();
                foreach (TreeNode selectedNode in treeModel.SelectedNodes)
                {
                    ModelMeshReference modelRef = (ModelMeshReference)selectedNode.Tag;
                    tempCopyGma.Add(gma[modelRef.ModelIdx]);
                    
                }
                tempCopyGma.Save(memoryCopy, GetSelectedGame());
                memoryCopy.Position = 0;
                tempCopyGma = new Gma(memoryCopy, GetSelectedGame());
                foreach (GmaEntry entry in tempCopyGma)
                {
                    gma.Add(entry);
                }
            }
                     
            UpdateModelDisplay();
            UpdateModelButtons();
            UpdateModelTree();

            UpdateMaterialList();
            UpdateMaterialDisplay();

            UpdateTextureDisplay();

            reloadOnNextRedraw = true;
            glControlModel.Invalidate();
        }

        private void showValuesAsHexadecimalToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            hexadecimalNumbers = showValuesAsHexadecimalToolStripMenuItem.Checked;
            UpdateModelDisplay();
            UpdateModelTree();
            UpdateMaterialList();
            UpdateTextureTree();
        }

        private void backgroundColorForTextureViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (texViewerColorDialog.ShowDialog() == DialogResult.OK)
            {
                pbTextureImage.BackColor = texViewerColorDialog.Color;
            }
        }

        /// <summary>
        /// Deletes a texture and corrects the texture index for all materials in the offset by the remoavl of the texture
        /// </summary>
        /// <param name="textureId">ID of the texture to remove</param>
        /// <param name="update">Whether or not to correct the texture indices of materials</param>
        public void DeleteTextureAt(int textureId, bool update = true)
        {
            int previousTextureId = (treeTextures.SelectedNode.Index) - 1;
            tpl.RemoveAt(textureId);

            if (update)
            {
                foreach (GmaEntry entry in gma)
                {
                    if (entry != null)
                    {
                        foreach (GcmfMaterial mat in entry.ModelObject.Materials)
                        {
                            if (mat.TextureIdx > textureId)
                            {
                                mat.TextureIdx--;
                            }
                        }
                    }
                }
            }

            UpdateTextureTree();
            UpdateTextureDisplay();
            UpdateTextureButtons();
            reloadOnNextRedraw = true;
            haveUnsavedTplChanges = true;

            if (treeTextures.Nodes.Count > 0)
            {
                if (previousTextureId > 0) treeTextures.SelectedNode = treeTextures.Nodes[previousTextureId];
                else treeTextures.SelectedNode = treeTextures.Nodes[0];
            }

        }

        /// <summary>
        /// Removes textures that do not have any associated models
        /// </summary>
        public void DeleteUnusedTextures()
        {
            // Probably a crappy way of doing this, and it's slow with lots of textures, but it works and has no issues with duplicate textures
            // Adding a method to delete a texture using a TplTexture as an argument might allow this to not be inefficient
            UpdateTexturesUsedBy();
            for (int texId = 0; texId < tpl.Count; texId++)
            {
                if (tpl[texId].usedByModels == null || tpl[texId].usedByModels.Count == 0)
                {
                    DeleteTextureAt(texId);
                    texId = -1;
                }
            }
        }
    }
}
