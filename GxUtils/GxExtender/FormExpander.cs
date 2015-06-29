using LibGxFormat;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;

namespace GxExpander
{
    public partial class FormExpander : Form
    {
        public FormExpander()
        {
            InitializeComponent();
            UpdateStartStopButtonText();

            // Populate ComboBox values from GxGame enum dynamically.
            cmbGame.ValueMember = "Key";
            cmbGame.DisplayMember = "Value";
            cmbGame.DataSource = new BindingSource(Enum.GetValues(typeof(GxGame)).Cast<GxGame>()
                .Select(g => new { Key = g, Value = EnumUtils.GetEnumDescription(g) }).ToArray(), null);
        }

        private void btnSelectInputPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogSelectInputPath.ShowDialog() == DialogResult.OK)
            {
                txtInputPath.Text = folderBrowserDialogSelectInputPath.SelectedPath;
            }
        }

        private void btnSelectOutputPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogSelectOutputPath.ShowDialog() == DialogResult.OK)
            {
                txtOutputPath.Text = folderBrowserDialogSelectOutputPath.SelectedPath;
            }
        }

        private void UpdateStartStopButtonText()
        {
            btnStartStop.Text = !backgroundWorkerPackUnpack.IsBusy ? "Start" : "Stop";
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!backgroundWorkerPackUnpack.IsBusy)
                {
                    GxExpander expander = new GxExpander();
                    expander.Mode = radioButtonModeUnpack.Checked ? GxExpanderMode.Unpack : GxExpanderMode.Pack;
                    expander.Game = (GxGame)cmbGame.SelectedValue;
                    expander.InputPath = txtInputPath.Text;
                    expander.OutputPath = txtOutputPath.Text;
                    expander.BackgroundWorker = backgroundWorkerPackUnpack;
                    backgroundWorkerPackUnpack.RunWorkerAsync(expander);
                    UpdateStartStopButtonText();
                }
                else
                {
                    backgroundWorkerPackUnpack.CancelAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error starting the process", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorkerPackUnpack_DoWork(object sender, DoWorkEventArgs e)
        {
            GxExpander expanderForBackgroundWorker = (GxExpander)e.Argument;
            expanderForBackgroundWorker.Run();
        }

        private void backgroundWorkerPackUnpack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, e.Error.Message, "Error while processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateStartStopButtonText();
        }
    }
}
