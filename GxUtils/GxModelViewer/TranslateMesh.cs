using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace GxModelViewer
{
    public partial class TranslateMesh : Form
    {
        public Vector3 translation;
        private Vector3 initialValues;
        private bool singleModel;

        public TranslateMesh()
        {
            InitializeComponent();
        }

        public void validateInput()
        {
            bool xValid = FlagHelper.parseFloat(this.xText.Text, out translation.X, "X is not a valid float value");
            bool yValid = FlagHelper.parseFloat(this.yText.Text, out translation.Y, "Y is not a valid float value");
            bool zValid = FlagHelper.parseFloat(this.zText.Text, out translation.Z, "Z is not a valid float value");
        }

        public void setInitial(Vector3 initialValues)
        {
            this.initialValues = initialValues;
            this.xText.Text = initialValues.X.ToString();
            this.yText.Text = initialValues.Y.ToString();
            this.zText.Text = initialValues.Z.ToString();
            infoText.Text = "Enter a new position: ";
            singleModel = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                validateInput();
                if (singleModel) translation -= initialValues;
            }

            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
