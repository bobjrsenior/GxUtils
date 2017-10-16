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
        GcmfMesh mesh;

        public MeshFlagEditor()
        {
            InitializeComponent();
        }

        public MeshFlagEditor(GcmfMesh mesh)
        {
            InitializeComponent();
            this.mesh = mesh;

            this.renderFlagsTextBox.Text = string.Format("{0:X8}", (uint)mesh.RenderFlags);
            this.layerTextBox.Text = "" + (int)mesh.Layer;
            this.unknown4TextBox.Text = String.Format("{0:X8}", mesh.Unk4);
            this.unknown8TextBox.Text = String.Format("{0:X8}", mesh.Unk8);
            this.unknownCTextBox.Text = String.Format("{0:X8}", mesh.UnkC);
            this.unknown10TextBox.Text = String.Format("{0:X4}", mesh.Unk10);
            //this.sectionFlagsTextBox.Text = String.Format("{0:X}", mesh.);
            this.unknown14TextBox.Text = String.Format("{0:X4}", mesh.Unk14);
            //this.vertexFlagsTextBox.Text = String.Format("{0:X}", mesh.);
            this.matrixId1TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[0];
            this.matrixId2TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[1];
            this.matrixId3TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[2];
            this.matrixId4TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[3];
            this.matrixId5TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[4];
            this.matrixId6TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[5];
            this.matrixId7TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[6];
            this.matrixId8TextBox.Text = "" + mesh.TransformMatrixSpecificIdxsObj1[7];
            // Second set of specific ids (To be implemented/normally don't exist)
            this.boundingSphereCenterXTextBox.Text = "" + mesh.BoundingSphereCenter.X;
            this.boundingSphereCenterYTextBox.Text = "" + mesh.BoundingSphereCenter.Y;
            this.boundingSphereCenterZTextBox.Text = "" + mesh.BoundingSphereCenter.Z;
            this.unknown3CTextBox.Text = "" +  mesh.Unk3C;
            this.unknown40TextBox.Text = String.Format("{0:X8}", mesh.Unk40);

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
            ushort unk10, unk14;
            uint renderFlags, unk4, unk8, unkC, unk40;
            uint layer;
            float boundingSphereX, boundingSphereY, boundingSphereZ, unk3C;
            if (!tryParseIntHex(this.renderFlagsTextBox.Text, out renderFlags)) throw new InvalidOperationException("Render Flags is not a valid 4 byte hex value");
            if (!tryParseIntHex(this.layerTextBox.Text, out layer)) throw new InvalidOperationException("Layer is not a valid 4 byte hex value");
            if (layer != 0 && layer != 1) throw new InvalidOperationException("Layer is not 0 or 1");
            if (!tryParseIntHex(this.unknown4TextBox.Text, out unk4)) throw new InvalidOperationException("Unknown 4 is not a valid 4 byte hex value");
            if (!tryParseIntHex(this.unknown8TextBox.Text, out unk8)) throw new InvalidOperationException("Unknown 8 is not a valid 4 byte hex value");
            if (!tryParseIntHex(this.unknownCTextBox.Text, out unkC)) throw new InvalidOperationException("Unknown C is not a valid 4 byte hex value");
            if (!tryParseShortHex(this.unknown10TextBox.Text, out unk10)) throw new InvalidOperationException("Unknown 10 is not a valid 4 byte hex value");
            if (!tryParseShortHex(this.unknown14TextBox.Text, out unk14)) throw new InvalidOperationException("Unknown 14 is not a valid 4 byte hex value");

            byte[] matrixSpecificIds = new byte[8];
            if (!byte.TryParse(this.matrixId1TextBox.Text, out matrixSpecificIds[0])) throw new InvalidOperationException("Transformation Matrix Specific Id One is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId2TextBox.Text, out matrixSpecificIds[1])) throw new InvalidOperationException("Transformation Matrix Specific Id Two is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId3TextBox.Text, out matrixSpecificIds[2])) throw new InvalidOperationException("Transformation Matrix Specific Id Three is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId4TextBox.Text, out matrixSpecificIds[3])) throw new InvalidOperationException("Transformation Matrix Specific Id Four is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId5TextBox.Text, out matrixSpecificIds[4])) throw new InvalidOperationException("Transformation Matrix Specific Id Five is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId6TextBox.Text, out matrixSpecificIds[5])) throw new InvalidOperationException("Transformation Matrix Specific Id Six is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId7TextBox.Text, out matrixSpecificIds[6])) throw new InvalidOperationException("Transformation Matrix Specific Id Seven is not a valid byte value (0-255)");
            if (!byte.TryParse(this.matrixId8TextBox.Text, out matrixSpecificIds[7])) throw new InvalidOperationException("Transformation Matrix Specific Id Eight is not a valid byte value (0-255)");

            if (!float.TryParse(this.boundingSphereCenterXTextBox.Text, out boundingSphereX)) throw new InvalidOperationException("Bounding Sphere Center X is not a valid float value");
            if (!float.TryParse(this.boundingSphereCenterYTextBox.Text, out boundingSphereY)) throw new InvalidOperationException("Bounding Sphere Center Y is not a valid float value");
            if (!float.TryParse(this.boundingSphereCenterZTextBox.Text, out boundingSphereZ)) throw new InvalidOperationException("Bounding Sphere Center Z is not a valid float value");

            if (!float.TryParse(this.unknown3CTextBox.Text, out unk3C)) throw new InvalidOperationException("Unknown 3C is not a valid float value");
            if (!tryParseIntHex(this.unknown40TextBox.Text, out unk40)) throw new InvalidOperationException("Unknown 40 is not a valid 4 byte hex value");

            mesh.RenderFlags = (GcmfMesh.RenderFlag)renderFlags;
            mesh.Layer = (GcmfMesh.MeshLayer)layer;
            mesh.Unk4 = unk4;
            mesh.Unk8 = unk8;
            mesh.UnkC = unkC;
            mesh.Unk10 = unk10;
            mesh.Unk14 = unk14;

            for (int i = 0; i < matrixSpecificIds.Length; i++)
            {
                mesh.TransformMatrixSpecificIdxsObj1[i] = matrixSpecificIds[i];
            }

            mesh.BoundingSphereCenter = new OpenTK.Vector3(boundingSphereX, boundingSphereY, boundingSphereZ);

            mesh.Unk3C = unk3C;
            mesh.Unk40 = unk40;

        }

        private bool tryParseShortHex(string hex, out ushort hexValue)
        {
            try
            {
                hexValue = Convert.ToUInt16(hex, 16);
                return true;
            }
            catch(Exception ex)
            {
                hexValue = 0;
                return false;
            }
        }

        private bool tryParseIntHex(string hex, out uint hexValue)
        {
            try
            {
                hexValue = Convert.ToUInt32(hex, 16);
                return true;
            }
            catch (Exception ex)
            {
                hexValue = 0;
                return false;
            }
        }
    }
}
