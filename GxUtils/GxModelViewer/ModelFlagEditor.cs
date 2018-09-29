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
        private const string ERROR_VALUE = "CONF";


        //private Gcmf model;
        private List<Gcmf> models;

        public ModelFlagEditor()
        {
            InitializeComponent();
        }

        public ModelFlagEditor(List<Gcmf> models)
        {
            InitializeComponent();
            this.models = models;
            
            this.sectionFlagsTextBox.Text = String.Format("{0:X8}", models[0].SectionFlags);

            this.boundingSphereCenterX.Text = "" + models[0].BoundingSphereCenter.X;
            this.boundingSphereCenterY.Text = "" + models[0].BoundingSphereCenter.Y;
            this.boundingSphereCenterZ.Text = "" + models[0].BoundingSphereCenter.Z;
            this.boundingSphereRadius.Text = "" + models[0].BoundingSphereRadius;

            this.transformationMatrixCount.Text = "" + models[0].TransformMatrices.Count;

            this.transformationMatrixDefaultIdOne.Text = "" + models[0].TransformMatrixDefaultIdxs[0];
            this.transformationMatrixDefaultIdTwo.Text = "" + models[0].TransformMatrixDefaultIdxs[1];
            this.transformationMatrixDefaultIdThree.Text = "" + models[0].TransformMatrixDefaultIdxs[2];
            this.transformationMatrixDefaultIdFour.Text = "" + models[0].TransformMatrixDefaultIdxs[3];
            this.transformationMatrixDefaultIdFive.Text = "" + models[0].TransformMatrixDefaultIdxs[4];
            this.transformationMatrixDefaultIdSix.Text = "" + models[0].TransformMatrixDefaultIdxs[5];
            this.transformationMatrixDefaultIdSeven.Text = "" + models[0].TransformMatrixDefaultIdxs[6];
            this.transformationMatrixDefaultIdEight.Text = "" + models[0].TransformMatrixDefaultIdxs[7];

            for(int i = 0; i < models.Count; i++)
            {
                if(models[i].SectionFlags != models[0].SectionFlags) this.sectionFlagsTextBox.Text = ERROR_VALUE;

                if (models[i].BoundingSphereCenter != models[0].BoundingSphereCenter)
                {
                    if (models[i].BoundingSphereCenter.X != models[0].BoundingSphereCenter.X) this.boundingSphereCenterX.Text = ERROR_VALUE;
                    if (models[i].BoundingSphereCenter.Y != models[0].BoundingSphereCenter.Y) this.boundingSphereCenterY.Text = ERROR_VALUE;
                    if (models[i].BoundingSphereCenter.Z != models[0].BoundingSphereCenter.Z) this.boundingSphereCenterZ.Text = ERROR_VALUE;
                }
                if (models[i].BoundingSphereRadius != models[0].BoundingSphereRadius) this.boundingSphereRadius.Text = ERROR_VALUE;


                if (models[i].TransformMatrixDefaultIdxs[0] != models[0].TransformMatrixDefaultIdxs[0]) this.transformationMatrixDefaultIdOne.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[1] != models[0].TransformMatrixDefaultIdxs[1]) this.transformationMatrixDefaultIdTwo.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[2] != models[0].TransformMatrixDefaultIdxs[2]) this.transformationMatrixDefaultIdThree.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[3] != models[0].TransformMatrixDefaultIdxs[3]) this.transformationMatrixDefaultIdFour.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[4] != models[0].TransformMatrixDefaultIdxs[4]) this.transformationMatrixDefaultIdFive.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[5] != models[0].TransformMatrixDefaultIdxs[5]) this.transformationMatrixDefaultIdSix.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[6] != models[0].TransformMatrixDefaultIdxs[6]) this.transformationMatrixDefaultIdSeven.Text = ERROR_VALUE;
                if (models[i].TransformMatrixDefaultIdxs[7] != models[0].TransformMatrixDefaultIdxs[7]) this.transformationMatrixDefaultIdEight.Text = ERROR_VALUE;
            }
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
            bool sectionFlagsValid;
            sectionFlagsValid = parseHexToInt32(this.sectionFlagsTextBox.Text, out sectionFlags, "Section Flags is not a valid 4 byte hex value");

            float boundingSphereX, boundingSphereY, boundingSphereZ, boundingSphereRadius;
            bool boundingSphereXValid, boundingSphereYValid, boundingSphereZValid, boundingSphereRadiusValid;
            boundingSphereXValid = parseFloat(this.boundingSphereCenterX.Text, out boundingSphereX, "Bounding Sphere Center X is not a valid float value");
            boundingSphereYValid = parseFloat(this.boundingSphereCenterY.Text, out boundingSphereY, "Bounding Sphere Center Y is not a valid float value");
            boundingSphereZValid = parseFloat(this.boundingSphereCenterZ.Text, out boundingSphereZ, "Bounding Sphere Center Z is not a valid float value");
            boundingSphereRadiusValid = parseFloat(this.boundingSphereRadius.Text, out boundingSphereRadius, "Bounding Sphere Radius is not a valid float value");

            byte[] matrixDefaultIds = new byte[8];
            bool[] matrixDefaultIdsValid = new bool[8];
            matrixDefaultIdsValid[0] = parseByte(this.transformationMatrixDefaultIdOne.Text, out matrixDefaultIds[0], "Transformation Matrix Default Id One is not a valid byte value (0-255)");
            matrixDefaultIdsValid[1] = parseByte(this.transformationMatrixDefaultIdTwo.Text, out matrixDefaultIds[1], "Transformation Matrix Default Id Two is not a valid byte value (0-255)");
            matrixDefaultIdsValid[2] = parseByte(this.transformationMatrixDefaultIdThree.Text, out matrixDefaultIds[2], "Transformation Matrix Default Id Three is not a valid byte value (0-255)");
            matrixDefaultIdsValid[3] = parseByte(this.transformationMatrixDefaultIdFour.Text, out matrixDefaultIds[3], "Transformation Matrix Default Id Four is not a valid byte value (0-255)");
            matrixDefaultIdsValid[4] = parseByte(this.transformationMatrixDefaultIdFive.Text, out matrixDefaultIds[4], "Transformation Matrix Default Id Five is not a valid byte value (0-255)");
            matrixDefaultIdsValid[5] = parseByte(this.transformationMatrixDefaultIdSix.Text, out matrixDefaultIds[5], "Transformation Matrix Default Id Six is not a valid byte value (0-255)");
            matrixDefaultIdsValid[6] = parseByte(this.transformationMatrixDefaultIdSeven.Text, out matrixDefaultIds[6], "Transformation Matrix Default Id Seven is not a valid byte value (0-255)");
            matrixDefaultIdsValid[7] = parseByte(this.transformationMatrixDefaultIdEight.Text, out matrixDefaultIds[7], "Transformation Matrix Default Id Eight is not a valid byte value (0-255)");


            foreach (Gcmf model in models)
            {
                // Copy new values
                if(sectionFlagsValid) model.SectionFlags = sectionFlags;

                float bSphereX, bSphereY, bSphereZ;
                if (boundingSphereXValid) bSphereX = boundingSphereX;
                else bSphereX = model.BoundingSphereCenter.X;
                if (boundingSphereYValid) bSphereY = boundingSphereY;
                else bSphereY = model.BoundingSphereCenter.Y;
                if (boundingSphereZValid) bSphereZ = boundingSphereZ;
                else bSphereZ = model.BoundingSphereCenter.Z;
                model.BoundingSphereCenter = new OpenTK.Vector3(bSphereX, bSphereY, bSphereZ);
                if(boundingSphereRadiusValid) model.BoundingSphereRadius = boundingSphereRadius;

                for (int i = 0; i < matrixDefaultIds.Length; i++)
                {
                    if(matrixDefaultIdsValid[i]) model.TransformMatrixDefaultIdxs[i] = matrixDefaultIds[i];
                }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {

            // Request image filename
            if (saveFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;

            StringBuilder sb = new StringBuilder();

            if (this.sectionFlagsTextBox.Text != ERROR_VALUE) sb.Append(SECTION_FLAGS).Append(" ").Append(this.sectionFlagsTextBox.Text).Append("\r\n");

            if (this.boundingSphereCenterX.Text != ERROR_VALUE) sb.Append(BOUNDING_SPHERE_CENTER_X).Append(" ").Append(this.boundingSphereCenterX.Text).Append("\r\n");
            if (this.boundingSphereCenterY.Text != ERROR_VALUE) sb.Append(BOUNDING_SPHERE_CENTER_Y).Append(" ").Append(this.boundingSphereCenterY.Text).Append("\r\n");
            if (this.boundingSphereCenterZ.Text != ERROR_VALUE) sb.Append(BOUNDING_SPHERE_CENTER_Z).Append(" ").Append(this.boundingSphereCenterZ.Text).Append("\r\n");
            if (this.boundingSphereRadius.Text != ERROR_VALUE) sb.Append(BOUNDING_SPHERE_RADIUS).Append(" ").Append(this.boundingSphereRadius.Text).Append("\r\n");

            if (this.transformationMatrixDefaultIdOne.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_ONE).Append(" ").Append(this.transformationMatrixDefaultIdOne.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdTwo.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_TWO).Append(" ").Append(this.transformationMatrixDefaultIdTwo.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdThree.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_THREE).Append(" ").Append(this.transformationMatrixDefaultIdThree.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdFour.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_FOUR).Append(" ").Append(this.transformationMatrixDefaultIdFour.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdFive.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_FIVE).Append(" ").Append(this.transformationMatrixDefaultIdFive.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdSix.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_SIX).Append(" ").Append(this.transformationMatrixDefaultIdSix.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdSeven.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_SEVEN).Append(" ").Append(this.transformationMatrixDefaultIdSeven.Text).Append("\r\n");
            if (this.transformationMatrixDefaultIdEight.Text != ERROR_VALUE) sb.Append(TRANSFORMATION_MATRIX_DEFAULT_ID_EIGHT).Append(" ").Append(this.transformationMatrixDefaultIdEight.Text).Append("\r\n");


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
                            case BOUNDING_SPHERE_RADIUS:
                                this.boundingSphereRadius.Text = line[1];
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

        /// <summary>
        /// A wrapper over float.parseFloat. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid float value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">float to store the result in</param>
        /// <returns></returns>
        private bool parseFloat(string s, out float result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = -1;
                return false;
            }
            else
            {
                if (!float.TryParse(s, out result)) throw new InvalidOperationException(throwText);

                return true;
            }
        }

        /// <summary>
        /// A wrapper over byte.tryParse. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid byte value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">byte to store the result in</param>
        /// <returns></returns>
        private bool parseByte(string s, out byte result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = 255;
                return false;
            }
            else
            {
                if (!byte.TryParse(s, out result)) throw new InvalidOperationException(throwText);

                return true;
            }
        }

        /// <summary>
        /// A wrapper over byte.tryParse. This method is similar except
        /// that it returns false if the string value is ERROR_VALUE and throws
        /// and InvalidOperationException if the string isn't a valid int32 value.
        /// The exception using the throwText parameter as it's message
        /// </summary>
        /// <param name="s">string to parse</param>
        /// <param name="result">byte to store the result in</param>
        /// <returns></returns>
        private bool parseHexToInt32(string s, out uint result, string throwText)
        {
            if (s.Equals(ERROR_VALUE))
            {
                result = uint.MaxValue;
                return false;
            }
            else
            {
                try
                {
                    result = (uint)Convert.ToInt32(this.sectionFlagsTextBox.Text, 16);
                }
                catch
                {
                    throw new InvalidOperationException(throwText);
                }
                return true;
            }
        }
    }
}
