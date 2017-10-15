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
    public partial class ModelFlagEditor : Form
    {
        private Gcmf model;

        public ModelFlagEditor()
        {
            InitializeComponent();
        }

        public ModelFlagEditor(Gcmf model)
        {
            InitializeComponent();
            this.model = model;
            this.sectionFlagsTextBox.Text = String.Format("{0:X}", model.SectionFlags);

            this.boundingSphereCenterX.Text = "" + model.BoundingSphereCenter.X;
            this.boundingSphereCenterY.Text = "" + model.BoundingSphereCenter.Y;
            this.boundingSphereCenterZ.Text = "" + model.BoundingSphereCenter.Z;
            this.boundingSphereRadius.Text = "" + model.BoundingSphereRadius;

            this.transformationMatrixCount.Text = "" + model.TransformMatrices.Count;

            this.transformationMatrixDefaultIdOne.Text = "" + model.TransformMatrixDefaultIdxs[0];
            this.transformationMatrixDefaultIdTwo.Text = "" + model.TransformMatrixDefaultIdxs[1];
            this.transformationMatrixDefaultIdThree.Text = "" + model.TransformMatrixDefaultIdxs[2];
            this.transformationMatrixDefaultIdFour.Text = "" + model.TransformMatrixDefaultIdxs[3];
            this.transformationMatrixDefaultIdFive.Text = "" + model.TransformMatrixDefaultIdxs[4];
            this.transformationMatrixDefaultIdSix.Text = "" + model.TransformMatrixDefaultIdxs[5];
            this.transformationMatrixDefaultIdSeven.Text = "" + model.TransformMatrixDefaultIdxs[6];
            this.transformationMatrixDefaultIdEight.Text = "" + model.TransformMatrixDefaultIdxs[7];
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            try {
                validateInput();
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show("The flags could not be updates: " + ex.Message,
                    "Error updating flags.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void validateInput()
        {
            // Validate input
            uint sectionFlags;
            try
            {
                sectionFlags = (uint)Convert.ToInt32(this.sectionFlagsTextBox.Text, 16);
            }
            catch
            {
                throw new InvalidOperationException("Section Flags is not a valid 4 byte hex value");
            }
            float boundingSphereX, boundingSphereY, boundingSphereZ, boundingSphereRadius;
            if (!float.TryParse(this.boundingSphereCenterX.Text, out boundingSphereX)) throw new InvalidOperationException("Bounding Sphere Center X is not a valid float value");
            if (!float.TryParse(this.boundingSphereCenterY.Text, out boundingSphereY)) throw new InvalidOperationException("Bounding Sphere Center Y is not a valid float value");
            if (!float.TryParse(this.boundingSphereCenterZ.Text, out boundingSphereZ)) throw new InvalidOperationException("Bounding Sphere Center Z is not a valid float value");
            if (!float.TryParse(this.boundingSphereRadius.Text, out boundingSphereRadius)) throw new InvalidOperationException("Bounding Sphere Radius is not a valid float value");

            byte[] matrixDefaultIds = new byte[8];
            if (!byte.TryParse(this.transformationMatrixDefaultIdOne.Text, out matrixDefaultIds[0])) throw new InvalidOperationException("Transformation Matrix Default Id One is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdTwo.Text, out matrixDefaultIds[1])) throw new InvalidOperationException("Transformation Matrix Default Id Two is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdThree.Text, out matrixDefaultIds[2])) throw new InvalidOperationException("Transformation Matrix Default Id Three is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdFour.Text, out matrixDefaultIds[3])) throw new InvalidOperationException("Transformation Matrix Default Id Four is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdFive.Text, out matrixDefaultIds[4])) throw new InvalidOperationException("Transformation Matrix Default Id Five is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdSix.Text, out matrixDefaultIds[5])) throw new InvalidOperationException("Transformation Matrix Default Id Six is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdSeven.Text, out matrixDefaultIds[6])) throw new InvalidOperationException("Transformation Matrix Default Id Seven is not a valid byte value (0-255)");
            if (!byte.TryParse(this.transformationMatrixDefaultIdEight.Text, out matrixDefaultIds[7])) throw new InvalidOperationException("Transformation Matrix Default Id Eight is not a valid byte value (0-255)");


            // Copy new values
            model.SectionFlags = sectionFlags;
            model.BoundingSphereCenter = new OpenTK.Vector3(boundingSphereX, boundingSphereY, boundingSphereZ);
            model.BoundingSphereRadius = boundingSphereRadius;

            for (int i = 0; i < matrixDefaultIds.Length; i++)
            {
                model.TransformMatrixDefaultIdxs[i] = matrixDefaultIds[i];
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
