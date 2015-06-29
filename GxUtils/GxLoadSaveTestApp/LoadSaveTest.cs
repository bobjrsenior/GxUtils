using LibGxFormat;
using LibGxFormat.Arc;
using LibGxFormat.Gma;
using LibGxFormat.Lz;
using LibGxFormat.Tpl;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GxLoadSaveTestApp
{
    public partial class LoadSaveTest : Form
    {
        struct RunTestsWorkerParams
        {
            public string inputPath;
            public GxGame game;
            public bool runLzTest;
            public bool runArcTest;
            public bool runTplTest;
            public bool runGmaTest;
            public bool saveDiffErrors;
        }

        public LoadSaveTest()
        {
            InitializeComponent();

            // Populate ComboBox values from GxGame enum dynamically.
            cmbGame.ValueMember = "Key";
            cmbGame.DisplayMember = "Value";
            cmbGame.DataSource = new BindingSource(Enum.GetValues(typeof(GxGame)).Cast<GxGame>()
                .Select(g => new { Key = g, Value = EnumUtils.GetEnumDescription(g) }).ToArray(), null);
        }

        private void btnSelectInputPath_Click(object sender, EventArgs e)
        {
            folderBrowserDialogSelectInputPath.SelectedPath = txtInputPath.Text;
            if (folderBrowserDialogSelectInputPath.ShowDialog() == DialogResult.OK)
                txtInputPath.Text = folderBrowserDialogSelectInputPath.SelectedPath;
        }

        private void btnStartTest_Click(object sender, EventArgs e)
        {
            // Check that the parameters given by the user are valid
            if (txtInputPath.Text.Length == 0)
            {
                MessageBox.Show("Please select an input path.", "Invalid parameters.",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!chkLoadSaveLzTest.Checked && !chkLoadSaveArcTest.Checked && 
                !chkLoadSaveTplTest.Checked && !chkLoadSaveGmaTest.Checked)
            {
                MessageBox.Show("Please select a test to run.", "Invalid parameters.",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Start a BackgroundWorker which will run the tests asynchronously
            backgroundWorkerRunTests.RunWorkerAsync(new RunTestsWorkerParams
            {
                inputPath = txtInputPath.Text,
                game = (GxGame)cmbGame.SelectedValue,
                runLzTest = chkLoadSaveLzTest.Checked,
                runArcTest = chkLoadSaveArcTest.Checked,
                runTplTest = chkLoadSaveTplTest.Checked,
                runGmaTest = chkLoadSaveGmaTest.Checked,
                saveDiffErrors = chkSaveDiffErrors.Checked
            });

            btnStartTest.Enabled = false;
            btnStopTest.Enabled = true;
        }

        private void btnStopTest_Click(object sender, EventArgs e)
        {
            backgroundWorkerRunTests.CancelAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void backgroundWorkerRunTests_DoWork(object sender, DoWorkEventArgs e)
        {
            RunTestsWorkerParams args = (RunTestsWorkerParams)e.Argument;
            
            // Find the list of files to process in the input directory
            string[] fileNames = Directory.EnumerateFiles(args.inputPath, "*.*", SearchOption.AllDirectories).Where(fn =>
                (args.runLzTest && fn.EndsWith(".lz")) ||
                (args.runArcTest && fn.EndsWith(".arc")) ||
                (args.runTplTest && fn.EndsWith(".tpl")) ||
                (args.runGmaTest && fn.EndsWith(".gma"))).ToArray();

            // Process each file
            int nErrors = 0;

            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = fileNames[i];

                byte[] originalFileData = File.ReadAllBytes(fileName);

                try
                {
                    // Load and resave the file using the appropiate class
                    MemoryStream originalStream = new MemoryStream(originalFileData);
                    MemoryStream resavedStream = new MemoryStream();

                    if (args.runLzTest && fileName.EndsWith(".lz"))
                    {
                        MemoryStream decodedStream = new MemoryStream();
                        Lz.Unpack(originalStream, decodedStream, args.game);
                        decodedStream.Position = 0;
                        Lz.Pack(decodedStream, resavedStream, args.game);
                    }
                    else if (args.runArcTest && fileName.EndsWith(".arc"))
                    {
                        ArcContainer arc = new ArcContainer(originalStream);
                        arc.Save(resavedStream);
                    }
                    else if (args.runTplTest && fileName.EndsWith(".tpl"))
                    {
                        Tpl tpl = new Tpl(originalStream, args.game);
                        tpl.Save(resavedStream, args.game);
                    }
                    else if (args.runGmaTest && fileName.EndsWith(".gma"))
                    {
                        Gma gma = new Gma(originalStream, args.game);
                        gma.Save(resavedStream, args.game);
                    }

                    // Check that the resaved file is equal to the original file
                    byte[] resavedFileData = resavedStream.ToArray();

                    if (!ByteArrayUtils.ByteArrayDataEquals(originalFileData, resavedFileData))
                    {
                        if (args.saveDiffErrors)
                            File.WriteAllBytes(fileName + "-error", resavedFileData);

                        throw new Exception("The resaved file is different than the original file.\n");
                    }
                }
                catch (Exception ex)
                {
                    txtLog.Invoke(new Action(() => txtLog.AppendText(
                        string.Format("{0}: {1}\n", fileName, ex.Message))));
                    nErrors++;
                }

                backgroundWorkerRunTests.ReportProgress(i * 100 / fileNames.Length);
                if (backgroundWorkerRunTests.CancellationPending)
                    return;
            }

            backgroundWorkerRunTests.ReportProgress(100);
            txtLog.Invoke(new Action(() => txtLog.AppendText(
                        string.Format("Test finished. {0} tested, {1} errors.\n", fileNames.Length, nErrors))));
        }

        private void backgroundWorkerRunTests_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbTestProgress.Value = e.ProgressPercentage;
        }

        private void backgroundWorkerRunTests_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(string.Format("An error ocurred during the tests: {0}.", e.Error.Message), "Error");

            btnStartTest.Enabled = true;
            btnStopTest.Enabled = false;
        }
    }
}
