namespace GxLoadSaveTestApp
{
    partial class LoadSaveTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanelParameters = new System.Windows.Forms.TableLayoutPanel();
            this.flwButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnStopTest = new System.Windows.Forms.Button();
            this.btnStartTest = new System.Windows.Forms.Button();
            this.lblInputPath = new System.Windows.Forms.Label();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.btnSelectInputPath = new System.Windows.Forms.Button();
            this.lblTests = new System.Windows.Forms.Label();
            this.chkLoadSaveLzTest = new System.Windows.Forms.CheckBox();
            this.chkLoadSaveTplTest = new System.Windows.Forms.CheckBox();
            this.chkLoadSaveGmaTest = new System.Windows.Forms.CheckBox();
            this.lblOptions = new System.Windows.Forms.Label();
            this.chkSaveDiffErrors = new System.Windows.Forms.CheckBox();
            this.lblGame = new System.Windows.Forms.Label();
            this.cmbGame = new System.Windows.Forms.ComboBox();
            this.pbTestProgress = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.chkLoadSaveArcTest = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialogSelectInputPath = new System.Windows.Forms.FolderBrowserDialog();
            this.backgroundWorkerRunTests = new System.ComponentModel.BackgroundWorker();
            this.tableLayoutPanelParameters.SuspendLayout();
            this.flwButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelParameters
            // 
            this.tableLayoutPanelParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelParameters.ColumnCount = 3;
            this.tableLayoutPanelParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanelParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelParameters.Controls.Add(this.flwButtons, 0, 9);
            this.tableLayoutPanelParameters.Controls.Add(this.lblInputPath, 0, 0);
            this.tableLayoutPanelParameters.Controls.Add(this.txtInputPath, 1, 0);
            this.tableLayoutPanelParameters.Controls.Add(this.btnSelectInputPath, 2, 0);
            this.tableLayoutPanelParameters.Controls.Add(this.lblTests, 0, 2);
            this.tableLayoutPanelParameters.Controls.Add(this.chkLoadSaveLzTest, 1, 2);
            this.tableLayoutPanelParameters.Controls.Add(this.chkLoadSaveTplTest, 1, 4);
            this.tableLayoutPanelParameters.Controls.Add(this.chkLoadSaveGmaTest, 1, 5);
            this.tableLayoutPanelParameters.Controls.Add(this.lblOptions, 0, 6);
            this.tableLayoutPanelParameters.Controls.Add(this.chkSaveDiffErrors, 1, 6);
            this.tableLayoutPanelParameters.Controls.Add(this.lblGame, 0, 1);
            this.tableLayoutPanelParameters.Controls.Add(this.cmbGame, 1, 1);
            this.tableLayoutPanelParameters.Controls.Add(this.pbTestProgress, 0, 7);
            this.tableLayoutPanelParameters.Controls.Add(this.txtLog, 0, 8);
            this.tableLayoutPanelParameters.Controls.Add(this.chkLoadSaveArcTest, 1, 3);
            this.tableLayoutPanelParameters.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanelParameters.Name = "tableLayoutPanelParameters";
            this.tableLayoutPanelParameters.RowCount = 10;
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelParameters.Size = new System.Drawing.Size(621, 365);
            this.tableLayoutPanelParameters.TabIndex = 0;
            // 
            // flwButtons
            // 
            this.flwButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelParameters.SetColumnSpan(this.flwButtons, 3);
            this.flwButtons.Controls.Add(this.btnClose);
            this.flwButtons.Controls.Add(this.btnStopTest);
            this.flwButtons.Controls.Add(this.btnStartTest);
            this.flwButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flwButtons.Location = new System.Drawing.Point(3, 333);
            this.flwButtons.Name = "flwButtons";
            this.flwButtons.Size = new System.Drawing.Size(615, 29);
            this.flwButtons.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(537, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnStopTest
            // 
            this.btnStopTest.Enabled = false;
            this.btnStopTest.Location = new System.Drawing.Point(456, 3);
            this.btnStopTest.Name = "btnStopTest";
            this.btnStopTest.Size = new System.Drawing.Size(75, 23);
            this.btnStopTest.TabIndex = 2;
            this.btnStopTest.Text = "Stop Test";
            this.btnStopTest.UseVisualStyleBackColor = true;
            this.btnStopTest.Click += new System.EventHandler(this.btnStopTest_Click);
            // 
            // btnStartTest
            // 
            this.btnStartTest.Location = new System.Drawing.Point(375, 3);
            this.btnStartTest.Name = "btnStartTest";
            this.btnStartTest.Size = new System.Drawing.Size(75, 23);
            this.btnStartTest.TabIndex = 0;
            this.btnStartTest.Text = "Start Test";
            this.btnStartTest.UseVisualStyleBackColor = true;
            this.btnStartTest.Click += new System.EventHandler(this.btnStartTest_Click);
            // 
            // lblInputPath
            // 
            this.lblInputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInputPath.AutoSize = true;
            this.lblInputPath.Location = new System.Drawing.Point(31, 0);
            this.lblInputPath.Name = "lblInputPath";
            this.lblInputPath.Size = new System.Drawing.Size(59, 13);
            this.lblInputPath.TabIndex = 0;
            this.lblInputPath.Text = "Input Path:";
            // 
            // txtInputPath
            // 
            this.txtInputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputPath.Location = new System.Drawing.Point(96, 3);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(428, 20);
            this.txtInputPath.TabIndex = 1;
            // 
            // btnSelectInputPath
            // 
            this.btnSelectInputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInputPath.Location = new System.Drawing.Point(530, 3);
            this.btnSelectInputPath.Name = "btnSelectInputPath";
            this.btnSelectInputPath.Size = new System.Drawing.Size(88, 24);
            this.btnSelectInputPath.TabIndex = 2;
            this.btnSelectInputPath.Text = "...";
            this.btnSelectInputPath.UseVisualStyleBackColor = true;
            this.btnSelectInputPath.Click += new System.EventHandler(this.btnSelectInputPath_Click);
            // 
            // lblTests
            // 
            this.lblTests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTests.AutoSize = true;
            this.lblTests.Location = new System.Drawing.Point(54, 60);
            this.lblTests.Name = "lblTests";
            this.lblTests.Size = new System.Drawing.Size(36, 13);
            this.lblTests.TabIndex = 3;
            this.lblTests.Text = "Tests:";
            // 
            // chkLoadSaveLzTest
            // 
            this.chkLoadSaveLzTest.AutoSize = true;
            this.tableLayoutPanelParameters.SetColumnSpan(this.chkLoadSaveLzTest, 2);
            this.chkLoadSaveLzTest.Location = new System.Drawing.Point(96, 63);
            this.chkLoadSaveLzTest.Name = "chkLoadSaveLzTest";
            this.chkLoadSaveLzTest.Size = new System.Drawing.Size(118, 17);
            this.chkLoadSaveLzTest.TabIndex = 4;
            this.chkLoadSaveLzTest.Text = "Load and Save .LZ";
            this.chkLoadSaveLzTest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkLoadSaveLzTest.UseVisualStyleBackColor = true;
            // 
            // chkLoadSaveTplTest
            // 
            this.chkLoadSaveTplTest.AutoSize = true;
            this.tableLayoutPanelParameters.SetColumnSpan(this.chkLoadSaveTplTest, 2);
            this.chkLoadSaveTplTest.Location = new System.Drawing.Point(96, 123);
            this.chkLoadSaveTplTest.Name = "chkLoadSaveTplTest";
            this.chkLoadSaveTplTest.Size = new System.Drawing.Size(125, 17);
            this.chkLoadSaveTplTest.TabIndex = 5;
            this.chkLoadSaveTplTest.Text = "Load and Save .TPL";
            this.chkLoadSaveTplTest.UseVisualStyleBackColor = true;
            // 
            // chkLoadSaveGmaTest
            // 
            this.chkLoadSaveGmaTest.AutoSize = true;
            this.tableLayoutPanelParameters.SetColumnSpan(this.chkLoadSaveGmaTest, 2);
            this.chkLoadSaveGmaTest.Location = new System.Drawing.Point(96, 153);
            this.chkLoadSaveGmaTest.Name = "chkLoadSaveGmaTest";
            this.chkLoadSaveGmaTest.Size = new System.Drawing.Size(129, 17);
            this.chkLoadSaveGmaTest.TabIndex = 6;
            this.chkLoadSaveGmaTest.Text = "Load and Save .GMA";
            this.chkLoadSaveGmaTest.UseVisualStyleBackColor = true;
            // 
            // lblOptions
            // 
            this.lblOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOptions.AutoSize = true;
            this.lblOptions.Location = new System.Drawing.Point(44, 180);
            this.lblOptions.Name = "lblOptions";
            this.lblOptions.Size = new System.Drawing.Size(46, 13);
            this.lblOptions.TabIndex = 7;
            this.lblOptions.Text = "Options:";
            // 
            // chkSaveDiffErrors
            // 
            this.chkSaveDiffErrors.AutoSize = true;
            this.tableLayoutPanelParameters.SetColumnSpan(this.chkSaveDiffErrors, 2);
            this.chkSaveDiffErrors.Location = new System.Drawing.Point(96, 183);
            this.chkSaveDiffErrors.Name = "chkSaveDiffErrors";
            this.chkSaveDiffErrors.Size = new System.Drawing.Size(251, 17);
            this.chkSaveDiffErrors.TabIndex = 8;
            this.chkSaveDiffErrors.Text = "If saved file differs, save erroneous output to file";
            this.chkSaveDiffErrors.UseVisualStyleBackColor = true;
            // 
            // lblGame
            // 
            this.lblGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGame.AutoSize = true;
            this.lblGame.Location = new System.Drawing.Point(52, 30);
            this.lblGame.Name = "lblGame";
            this.lblGame.Size = new System.Drawing.Size(38, 13);
            this.lblGame.TabIndex = 9;
            this.lblGame.Text = "Game:";
            // 
            // cmbGame
            // 
            this.tableLayoutPanelParameters.SetColumnSpan(this.cmbGame, 2);
            this.cmbGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGame.FormattingEnabled = true;
            this.cmbGame.Location = new System.Drawing.Point(96, 33);
            this.cmbGame.Name = "cmbGame";
            this.cmbGame.Size = new System.Drawing.Size(121, 21);
            this.cmbGame.TabIndex = 10;
            // 
            // pbTestProgress
            // 
            this.pbTestProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelParameters.SetColumnSpan(this.pbTestProgress, 3);
            this.pbTestProgress.Location = new System.Drawing.Point(3, 213);
            this.pbTestProgress.Name = "pbTestProgress";
            this.pbTestProgress.Size = new System.Drawing.Size(615, 24);
            this.pbTestProgress.TabIndex = 2;
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelParameters.SetColumnSpan(this.txtLog, 3);
            this.txtLog.Location = new System.Drawing.Point(3, 243);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(615, 84);
            this.txtLog.TabIndex = 2;
            // 
            // chkLoadSaveArcTest
            // 
            this.chkLoadSaveArcTest.AutoSize = true;
            this.chkLoadSaveArcTest.Location = new System.Drawing.Point(96, 93);
            this.chkLoadSaveArcTest.Name = "chkLoadSaveArcTest";
            this.chkLoadSaveArcTest.Size = new System.Drawing.Size(127, 17);
            this.chkLoadSaveArcTest.TabIndex = 4;
            this.chkLoadSaveArcTest.Text = "Load and Save .ARC";
            this.chkLoadSaveArcTest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkLoadSaveArcTest.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialogSelectInputPath
            // 
            this.folderBrowserDialogSelectInputPath.Description = "Select Input Path...";
            this.folderBrowserDialogSelectInputPath.ShowNewFolderButton = false;
            // 
            // backgroundWorkerRunTests
            // 
            this.backgroundWorkerRunTests.WorkerReportsProgress = true;
            this.backgroundWorkerRunTests.WorkerSupportsCancellation = true;
            this.backgroundWorkerRunTests.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerRunTests_DoWork);
            this.backgroundWorkerRunTests.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerRunTests_ProgressChanged);
            this.backgroundWorkerRunTests.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerRunTests_RunWorkerCompleted);
            // 
            // LoadSaveTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 389);
            this.Controls.Add(this.tableLayoutPanelParameters);
            this.Name = "LoadSaveTest";
            this.Text = "LibGxFormat - Load & Save Test";
            this.tableLayoutPanelParameters.ResumeLayout(false);
            this.tableLayoutPanelParameters.PerformLayout();
            this.flwButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelParameters;
        private System.Windows.Forms.Label lblInputPath;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.Button btnSelectInputPath;
        private System.Windows.Forms.Label lblTests;
        private System.Windows.Forms.CheckBox chkLoadSaveLzTest;
        private System.Windows.Forms.CheckBox chkLoadSaveTplTest;
        private System.Windows.Forms.CheckBox chkLoadSaveGmaTest;
        private System.Windows.Forms.Label lblOptions;
        private System.Windows.Forms.CheckBox chkSaveDiffErrors;
        private System.Windows.Forms.Label lblGame;
        private System.Windows.Forms.ComboBox cmbGame;
        private System.Windows.Forms.FlowLayoutPanel flwButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnStartTest;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogSelectInputPath;
        private System.ComponentModel.BackgroundWorker backgroundWorkerRunTests;
        private System.Windows.Forms.Button btnStopTest;
        private System.Windows.Forms.ProgressBar pbTestProgress;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.CheckBox chkLoadSaveArcTest;

    }
}