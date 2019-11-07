using GxModelViewer;
using LibGxFormat.Gma;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GxModelViewer
{
    public partial class MeshFlagEditor : Form
    {
        private const string RENDER_FLAGS = "RENDER_FLAGS";
        private const string LAYER = "LAYER";
        private const string UNKNOWN_4 = "UNKNOWN_4";
        private const string UNKNOWN_8 = "UNKNOWN_8";
        private const string UNKNOWN_C = "UNKNOWN_C";
        private const string UNKNOWN_10 = "UNKNOWN_10";
        private const string UNKNOWN_14 = "UNKNOWN_14";
        private const string MATRIX_SPECIFIC_IDS_ONE = "MATRIX_SPECIFIC_IDS_ONE";
        private const string MATRIX_SPECIFIC_IDS_TWO = "MATRIX_SPECIFIC_IDS_TWO";
        private const string MATRIX_SPECIFIC_IDS_THREE = "MATRIX_SPECIFIC_IDS_THREE";
        private const string MATRIX_SPECIFIC_IDS_FOUR = "MATRIX_SPECIFIC_IDS_FOUR";
        private const string MATRIX_SPECIFIC_IDS_FIVE = "MATRIX_SPECIFIC_IDS_FIVE";
        private const string MATRIX_SPECIFIC_IDS_SIX = "MATRIX_SPECIFIC_IDS_SIXE";
        private const string MATRIX_SPECIFIC_IDS_SEVEN = "MATRIX_SPECIFIC_IDS_SEVEN";
        private const string MATRIX_SPECIFIC_IDS_EIGHT = "MATRIX_SPECIFIC_IDS_EIGHT";
        private const string BOUNDING_SPHERE_CENTER_X = "BOUNDING_SPHERE_CENTER_X";
        private const string BOUNDING_SPHERE_CENTER_Y = "BOUNDING_SPHERE_CENTER_Y";
        private const string BOUNDING_SPHERE_CENTER_Z = "BOUNDING_SPHERE_CENTER_Z";
        private const string UNKNOWN_3C = "UNKNOWN_3C";
        private const string UNKNOWN_40 = "UNKNOWN_40";

        List<GcmfMesh> meshes;

        public MeshFlagEditor()
        {
            InitializeComponent();
        }

        public MeshFlagEditor(List<GcmfMesh> meshes)
        {
            InitializeComponent();
            this.meshes = meshes;

            this.renderFlagsTextBox.Text = string.Format("{0:X8}", (uint)meshes[0].RenderFlags);
            this.layerTextBox.Text = "" + (int)meshes[0].Layer;
            this.unknown4TextBox.Text = String.Format("{0:X8}", meshes[0].Unk4);
            this.unknown8TextBox.Text = String.Format("{0:X8}", meshes[0].Unk8);
            this.unknownCTextBox.Text = String.Format("{0:X8}", meshes[0].UnkC);
            this.unknown10TextBox.Text = String.Format("{0:X4}", meshes[0].Unk10);
            //this.sectionFlagsTextBox.Text = String.Format("{0:X}", meshes[0].);
            this.unknown14TextBox.Text = String.Format("{0:X4}", meshes[0].Unk14);
            this.unknown16TextBox.Text = String.Format("{0:X4}", meshes[0].PrimaryMaterialIdx);
            this.unknown18TextBox.Text = String.Format("{0:X4}", meshes[0].SecondaryMaterialIdx);
            this.unknown1ATextBox.Text = String.Format("{0:X4}", meshes[0].TertiaryMaterialIdx);
            //this.vertexFlagsTextBox.Text = String.Format("{0:X}", meshes[0].);
            this.matrixId1TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[0];
            this.matrixId2TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[1];
            this.matrixId3TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[2];
            this.matrixId4TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[3];
            this.matrixId5TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[4];
            this.matrixId6TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[5];
            this.matrixId7TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[6];
            this.matrixId8TextBox.Text = "" + meshes[0].TransformMatrixSpecificIdxsObj1[7];
            // Second set of specific ids (To be implemented/normally don't exist)
            this.boundingSphereCenterXTextBox.Text = "" + meshes[0].BoundingSphereCenter.X;
            this.boundingSphereCenterYTextBox.Text = "" + meshes[0].BoundingSphereCenter.Y;
            this.boundingSphereCenterZTextBox.Text = "" + meshes[0].BoundingSphereCenter.Z;
            this.unknown3CTextBox.Text = "" + meshes[0].Unk3C;
            this.unknown40TextBox.Text = String.Format("{0:X8}", meshes[0].Unk40);

            for (int i = 1; i < meshes.Count; i++)
            {
                if (meshes[i].RenderFlags != meshes[0].RenderFlags) this.renderFlagsTextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].Layer != meshes[0].Layer) this.layerTextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].Unk4 != meshes[0].Unk4) this.unknown4TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].Unk8 != meshes[0].Unk8) this.unknown8TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].UnkC != meshes[0].UnkC) this.unknownCTextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].Unk10 != meshes[0].Unk10) this.unknown10TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].Unk14 != meshes[0].Unk14) this.unknown14TextBox.Text = FlagHelper.ERROR_VALUE;

                if (meshes[i].TransformMatrixSpecificIdxsObj1[0] != meshes[0].TransformMatrixSpecificIdxsObj1[0]) this.matrixId1TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[1] != meshes[0].TransformMatrixSpecificIdxsObj1[1]) this.matrixId2TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[2] != meshes[0].TransformMatrixSpecificIdxsObj1[2]) this.matrixId3TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[3] != meshes[0].TransformMatrixSpecificIdxsObj1[3]) this.matrixId4TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[4] != meshes[0].TransformMatrixSpecificIdxsObj1[4]) this.matrixId5TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[5] != meshes[0].TransformMatrixSpecificIdxsObj1[5]) this.matrixId6TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[6] != meshes[0].TransformMatrixSpecificIdxsObj1[6]) this.matrixId7TextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].TransformMatrixSpecificIdxsObj1[7] != meshes[0].TransformMatrixSpecificIdxsObj1[7]) this.matrixId8TextBox.Text = FlagHelper.ERROR_VALUE;

                if (meshes[i].BoundingSphereCenter.X != meshes[0].BoundingSphereCenter.X) this.boundingSphereCenterXTextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].BoundingSphereCenter.Y != meshes[0].BoundingSphereCenter.Y) this.boundingSphereCenterYTextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].BoundingSphereCenter.Z != meshes[0].BoundingSphereCenter.Z) this.boundingSphereCenterZTextBox.Text = FlagHelper.ERROR_VALUE;

                if (meshes[i].Unk3C != meshes[0].Unk3C) this.unknown3CTextBox.Text = FlagHelper.ERROR_VALUE;
                if (meshes[i].Unk40 != meshes[0].Unk40) this.unknown40TextBox.Text = FlagHelper.ERROR_VALUE;
            }
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            try
            {
                validateInput();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("The flags could not be updated: " + ex.Message,
                    "Error updating flags.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void validateInput()
        {
            ushort unk10, unk14, unk16, unk18, unk1A;
            uint renderFlags, unk4, unk8, unkC, unk40;
            uint layer;
            bool unk10Valid, unk14Valid, unk16Valid, unk18Valid, unk1AValid, renderFlagsValid, unk4Valid, unk8Valid, unkCValid, unk40Valid, layerValid;
            float boundingSphereX, boundingSphereY, boundingSphereZ, unk3C;
            bool boundingSphereXValid, boundingSphereYValid, boundingSphereZValid, unk3CValid;

            renderFlagsValid = FlagHelper.parseHexToInt32(this.renderFlagsTextBox.Text, out renderFlags, "Render Flags is not a valid 4 byte hex value");
            layerValid = FlagHelper.parseHexToInt32(this.layerTextBox.Text, out layer, "Layer is not a valid 4 byte hex value");
            if (layer != 0 && layer != 1) throw new InvalidOperationException("Layer is not 0 or 1");
            unk4Valid = FlagHelper.parseHexToInt32(this.unknown4TextBox.Text, out unk4, "Vertex Shading A 0x04 is not a valid 4 byte hex value");
            unk8Valid = FlagHelper.parseHexToInt32(this.unknown8TextBox.Text, out unk8, "Vertex Shading B 0x08 is not a valid 4 byte hex value");
            unkCValid = FlagHelper.parseHexToInt32(this.unknownCTextBox.Text, out unkC, "Specular Tint 0x0C is not a valid 4 byte hex value");
            unk10Valid = FlagHelper.parseHexToShort(this.unknown10TextBox.Text, out unk10, "Transparency 0x10 is not a valid 2 byte hex value");
            unk14Valid = FlagHelper.parseHexToShort(this.unknown14TextBox.Text, out unk14, "Unknown 0x14 is not a valid 2 byte hex value");
            unk16Valid = FlagHelper.parseHexToShort(this.unknown16TextBox.Text, out unk16, "Primary Material Index 0x16 is not a valid 2 byte hex value");
            unk18Valid = FlagHelper.parseHexToShort(this.unknown18TextBox.Text, out unk18, "Secondary Material Index 0x18 is not a valid 2 byte hex value");
            unk1AValid = FlagHelper.parseHexToShort(this.unknown1ATextBox.Text, out unk1A, "Tertiary Material Index 0x1A is not a valid 2 byte hex value");
        
            byte[] matrixSpecificIds = new byte[8];
            bool[] matrixSpecificIdsValid = new bool[8];

            matrixSpecificIdsValid[0] = FlagHelper.parseByte(this.matrixId1TextBox.Text, out matrixSpecificIds[0], "Transformation Matrix Specific Id One is not a valid byte value (0-255)");
            matrixSpecificIdsValid[1] = FlagHelper.parseByte(this.matrixId2TextBox.Text, out matrixSpecificIds[1], "Transformation Matrix Specific Id Two is not a valid byte value (0-255)");
            matrixSpecificIdsValid[2] = FlagHelper.parseByte(this.matrixId3TextBox.Text, out matrixSpecificIds[2], "Transformation Matrix Specific Id Three is not a valid byte value (0-255)");
            matrixSpecificIdsValid[3] = FlagHelper.parseByte(this.matrixId4TextBox.Text, out matrixSpecificIds[3], "Transformation Matrix Specific Id Four is not a valid byte value (0-255)");
            matrixSpecificIdsValid[4] = FlagHelper.parseByte(this.matrixId5TextBox.Text, out matrixSpecificIds[4], "Transformation Matrix Specific Id Five is not a valid byte value (0-255)");
            matrixSpecificIdsValid[5] = FlagHelper.parseByte(this.matrixId6TextBox.Text, out matrixSpecificIds[5], "Transformation Matrix Specific Id Six is not a valid byte value (0-255)");
            matrixSpecificIdsValid[6] = FlagHelper.parseByte(this.matrixId7TextBox.Text, out matrixSpecificIds[6], "Transformation Matrix Specific Id Seven is not a valid byte value (0-255)");
            matrixSpecificIdsValid[7] = FlagHelper.parseByte(this.matrixId8TextBox.Text, out matrixSpecificIds[7], "Transformation Matrix Specific Id Eight is not a valid byte value (0-255)");

            boundingSphereXValid = FlagHelper.parseFloat(this.boundingSphereCenterXTextBox.Text, out boundingSphereX, "Bounding Sphere Center X is not a valid float value");
            boundingSphereYValid = FlagHelper.parseFloat(this.boundingSphereCenterYTextBox.Text, out boundingSphereY, "Bounding Sphere Center Y is not a valid float value");
            boundingSphereZValid = FlagHelper.parseFloat(this.boundingSphereCenterZTextBox.Text, out boundingSphereZ, "Bounding Sphere Center Z is not a valid float value");

            unk3CValid = FlagHelper.parseFloat(this.unknown3CTextBox.Text, out unk3C, "Unknown 0x3C is not a valid float value");
            unk40Valid = FlagHelper.parseHexToInt32(this.unknown40TextBox.Text, out unk40, "Unknown 0x40 is not a valid 4 byte hex value");


            foreach (GcmfMesh mesh in meshes)
            {
                if (renderFlagsValid) mesh.RenderFlags = (GcmfMesh.RenderFlag)renderFlags;
                if (layerValid) mesh.Layer = (GcmfMesh.MeshLayer)layer;
                if (unk4Valid) mesh.Unk4 = unk4;
                if (unk8Valid) mesh.Unk8 = unk8;
                if (unkCValid) mesh.UnkC = unkC;
                if (unk10Valid) mesh.Unk10 = unk10;
                if (unk14Valid) mesh.Unk14 = unk14;
                if (unk16Valid) mesh.PrimaryMaterialIdx = unk16;
                if (unk18Valid) mesh.SecondaryMaterialIdx = unk18;
                if (unk1AValid) mesh.TertiaryMaterialIdx = unk1A;
                mesh.calculatedUsedMaterialCount = Convert.ToByte(((unk16 != ushort.MaxValue) ? 1 : 0) +
                        ((unk18 != ushort.MaxValue) ? 1 : 0) +
                        ((unk1A != ushort.MaxValue) ? 1 : 0));

                for (int i = 0; i < matrixSpecificIds.Length; i++)
                {
                    if (matrixSpecificIdsValid[i]) mesh.TransformMatrixSpecificIdxsObj1[i] = matrixSpecificIds[i];
                }

                float bSphereX, bSphereY, bSphereZ;
                if (boundingSphereXValid) bSphereX = boundingSphereX;
                else bSphereX = mesh.BoundingSphereCenter.X;
                if (boundingSphereYValid) bSphereY = boundingSphereY;
                else bSphereY = mesh.BoundingSphereCenter.Y;
                if (boundingSphereZValid) bSphereZ = boundingSphereZ;
                else bSphereZ = mesh.BoundingSphereCenter.Z;
                mesh.BoundingSphereCenter = new OpenTK.Vector3(bSphereX, bSphereY, bSphereZ);

                if(unk3CValid) mesh.Unk3C = unk3C;
                if(unk40Valid) mesh.Unk40 = unk40;
            }

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Request image filename
            if (saveFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;

            StringBuilder sb = new StringBuilder();

            if (this.renderFlagsTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(RENDER_FLAGS).Append(" ").Append(this.renderFlagsTextBox.Text).Append("\r\n");
            if (this.layerTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(LAYER).Append(" ").Append(this.layerTextBox.Text).Append("\r\n");
            if (this.unknown4TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_4).Append(" ").Append(this.unknown4TextBox.Text).Append("\r\n");
            if (this.unknown8TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_8).Append(" ").Append(this.unknown8TextBox.Text).Append("\r\n");
            if (this.unknownCTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_C).Append(" ").Append(this.unknownCTextBox.Text).Append("\r\n");
            if (this.unknown10TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_10).Append(" ").Append(this.unknown10TextBox.Text).Append("\r\n");
            if (this.unknown14TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_14).Append(" ").Append(this.unknown14TextBox.Text).Append("\r\n");
            if (this.unknown16TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append("UNKNOWN_16").Append(" ").Append(this.unknown16TextBox.Text).Append("\r\n");
            if (this.unknown18TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append("UNKNOWN_18").Append(" ").Append(this.unknown18TextBox.Text).Append("\r\n");
            if (this.unknown1ATextBox.Text != FlagHelper.ERROR_VALUE) sb.Append("UNKNOWN_1A").Append(" ").Append(this.unknown1ATextBox.Text).Append("\r\n");
            if (this.matrixId1TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_ONE).Append(" ").Append(this.matrixId1TextBox.Text).Append("\r\n");
            if (this.matrixId2TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_TWO).Append(" ").Append(this.matrixId2TextBox.Text).Append("\r\n");
            if (this.matrixId3TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_THREE).Append(" ").Append(this.matrixId3TextBox.Text).Append("\r\n");
            if (this.matrixId4TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_FOUR).Append(" ").Append(this.matrixId4TextBox.Text).Append("\r\n");
            if (this.matrixId5TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_FIVE).Append(" ").Append(this.matrixId5TextBox.Text).Append("\r\n");
            if (this.matrixId6TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_SIX).Append(" ").Append(this.matrixId6TextBox.Text).Append("\r\n");
            if (this.matrixId7TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_SEVEN).Append(" ").Append(this.matrixId7TextBox.Text).Append("\r\n");
            if (this.matrixId8TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(MATRIX_SPECIFIC_IDS_EIGHT).Append(" ").Append(this.matrixId8TextBox.Text).Append("\r\n");

            if (this.boundingSphereCenterXTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(BOUNDING_SPHERE_CENTER_X).Append(" ").Append(this.boundingSphereCenterXTextBox.Text).Append("\r\n");
            if (this.boundingSphereCenterYTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(BOUNDING_SPHERE_CENTER_Y).Append(" ").Append(this.boundingSphereCenterYTextBox.Text).Append("\r\n");
            if (this.boundingSphereCenterZTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(BOUNDING_SPHERE_CENTER_Z).Append(" ").Append(this.boundingSphereCenterZTextBox.Text).Append("\r\n");

            if (this.unknown3CTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_3C).Append(" ").Append(this.unknown3CTextBox.Text).Append("\r\n");
            if (this.unknown40TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_40).Append(" ").Append(this.unknown40TextBox.Text).Append("\r\n");

            System.IO.File.WriteAllText(saveFlagsFileDialog.FileName, sb.ToString());
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            // Request image filename
            if (openFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(openFlagsFileDialog.FileName);

                List<string> flagWarningLog = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] line = lines[i].Split();
                    if (line.Length == 2)
                    {
                        switch (line[0])
                        {
                            case RENDER_FLAGS:
                                this.renderFlagsTextBox.Text = line[1];
                                break;
                            case LAYER:
                                this.layerTextBox.Text = line[1];
                                break;
                            case UNKNOWN_4:
                                this.unknown4TextBox.Text = line[1];
                                break;
                            case UNKNOWN_8:
                                this.unknown8TextBox.Text = line[1];
                                break;
                            case UNKNOWN_C:
                                this.unknownCTextBox.Text = line[1];
                                break;
                            case UNKNOWN_10:
                                this.unknown10TextBox.Text = line[1];
                                break;
                            case UNKNOWN_14:
                                this.unknown14TextBox.Text = line[1];
                                break;
                            case "UNKNOWN_16":
                                this.unknown16TextBox.Text = line[1];
                                break;
                            case "UNKNOWN_18":
                                this.unknown18TextBox.Text = line[1];
                                break;
                            case "UNKNOWN_1A":
                                this.unknown1ATextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_ONE:
                                this.matrixId1TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_TWO:
                                this.matrixId2TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_THREE:
                                this.matrixId3TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_FOUR:
                                this.matrixId4TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_FIVE:
                                this.matrixId5TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_SIX:
                                this.matrixId6TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_SEVEN:
                                this.matrixId7TextBox.Text = line[1];
                                break;
                            case MATRIX_SPECIFIC_IDS_EIGHT:
                                this.matrixId8TextBox.Text = line[1];
                                break;
                            case BOUNDING_SPHERE_CENTER_X:
                                this.boundingSphereCenterXTextBox.Text = line[1];
                                break;
                            case BOUNDING_SPHERE_CENTER_Y:
                                this.boundingSphereCenterYTextBox.Text = line[1];
                                break;
                            case BOUNDING_SPHERE_CENTER_Z:
                                this.boundingSphereCenterZTextBox.Text = line[1];
                                break;
                            case UNKNOWN_3C:
                                this.unknown3CTextBox.Text = line[1];
                                break;
                            case UNKNOWN_40:
                                this.unknown40TextBox.Text = line[1];
                                break;
                            default:
                                flagWarningLog.Add("Warning Unknown Flag: " + lines[i]);
                                break;
                        }
                    }
                    else
                    {
                        flagWarningLog.Add("Warning Invalid Flag Format: " + lines[i]);
                    }
                }

                if (flagWarningLog.Count != 0)
                {
                    FlagsWarningLogDialog warningDlg = new FlagsWarningLogDialog(flagWarningLog, "Mesh Flag Import Warnings", "The following warnings were issued while importing the mesh flags:");
                    if (warningDlg.ShowDialog() != DialogResult.Yes)
                        return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading the Flags file. " + ex.Message, "Error loading the Flags file.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}
