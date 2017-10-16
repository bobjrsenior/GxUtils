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
        private const string SECTION_FLAGS = "SECTION_FLAGS";
        private const string BOUNDING_SPHERE_CENTER_X = "BOUNDING_SPHERE_CENTER_X";
        private const string BOUNDING_SPHERE_CENTER_Y = "BOUNDING_SPHERE_CENTER_Y";
        private const string BOUNDING_SPHERE_CENTER_Z = "BOUNDING_SPHERE_CENTER_Z";
        private const string BOUNDING_SPHERE_RADIUS = "BOUNDING_SPHERE_RADIUS";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_ONE = "TRANSFORMATION_MATRIX_DEFAULT_ID_ONE";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_TWO = "TRANSFORMATION_MATRIX_DEFAULT_ID_TWO";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_THREE = "TRANSFORMATION_MATRIX_DEFAULT_ID_THREE";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_FOUR = "TRANSFORMATION_MATRIX_DEFAULT_ID_FOUR";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_FIVE = "TRANSFORMATION_MATRIX_DEFAULT_ID_FIVE";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_SIX = "TRANSFORMATION_MATRIX_DEFAULT_ID_SIX";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_SEVEN = "TRANSFORMATION_MATRIX_DEFAULT_ID_SEVEN";
        private const string TRANSFORMATION_MATRIX_DEFAULT_ID_EIGHT = "TRANSFORMATION_MATRIX_DEFAULT_ID_EIGHT";


        private Gcmf model;

        public ModelFlagEditor()
        {
            InitializeComponent();
        }

        public ModelFlagEditor(Gcmf model)
        {
            InitializeComponent();
            this.model = model;
            this.sectionFlagsTextBox.Text = String.Format("{0:X8}", model.SectionFlags);

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
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show("The flags could not be updated: " + ex.Message,
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
        }

        private void saveButton_Click(object sender, EventArgs e)
        {

            // Request image filename
            if (saveFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;

            StringBuilder sb = new StringBuilder()
                .Append(SECTION_FLAGS).Append(" ").Append(this.sectionFlagsTextBox.Text).Append("\r\n")
                .Append(BOUNDING_SPHERE_CENTER_X).Append(" ").Append(this.boundingSphereCenterX.Text).Append("\r\n")
                .Append(BOUNDING_SPHERE_CENTER_Y).Append(" ").Append(this.boundingSphereCenterY.Text).Append("\r\n")
                .Append(BOUNDING_SPHERE_CENTER_Z).Append(" ").Append(this.boundingSphereCenterZ.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_ONE).Append(" ").Append(this.transformationMatrixDefaultIdOne.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_TWO).Append(" ").Append(this.transformationMatrixDefaultIdTwo.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_THREE).Append(" ").Append(this.transformationMatrixDefaultIdThree.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_FOUR).Append(" ").Append(this.transformationMatrixDefaultIdFour.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_FIVE).Append(" ").Append(this.transformationMatrixDefaultIdFive.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_SIX).Append(" ").Append(this.transformationMatrixDefaultIdSix.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_SEVEN).Append(" ").Append(this.transformationMatrixDefaultIdSeven.Text).Append("\r\n")
                .Append(TRANSFORMATION_MATRIX_DEFAULT_ID_EIGHT).Append(" ").Append(this.transformationMatrixDefaultIdEight.Text);

            System.IO.File.WriteAllText(saveFlagsFileDialog.FileName, sb.ToString());
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            // Request image filename
            if (openFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try { 
                string[] lines = System.IO.File.ReadAllLines(openFlagsFileDialog.FileName);

                List<string> flagWarningLog = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] line = lines[i].Split();
                    if (line.Length == 2)
                    {
                        switch (line[0])
                        {
                            case SECTION_FLAGS:
                                this.sectionFlagsTextBox.Text = line[1];
                                break;
                            case BOUNDING_SPHERE_CENTER_X:
                                this.boundingSphereCenterX.Text = line[1];
                                break;
                            case BOUNDING_SPHERE_CENTER_Y:
                                this.boundingSphereCenterY.Text = line[1];
                                break;
                            case BOUNDING_SPHERE_CENTER_Z:
                                this.boundingSphereCenterZ.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_ONE:
                                this.transformationMatrixDefaultIdOne.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_TWO:
                                this.transformationMatrixDefaultIdTwo.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_THREE:
                                this.transformationMatrixDefaultIdThree.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_FOUR:
                                this.transformationMatrixDefaultIdFour.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_FIVE:
                                this.transformationMatrixDefaultIdFive.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_SIX:
                                this.transformationMatrixDefaultIdSix.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_SEVEN:
                                this.transformationMatrixDefaultIdSeven.Text = line[1];
                                break;
                            case TRANSFORMATION_MATRIX_DEFAULT_ID_EIGHT:
                                this.transformationMatrixDefaultIdEight.Text = line[1];
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
                    // TODO
                    //ObjMtlWarningLogDialog warningDlg = new ObjMtlWarningLogDialog(flagWarningLog);
                    //if (warningDlg.ShowDialog() != DialogResult.Yes)
                    //    return;
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
