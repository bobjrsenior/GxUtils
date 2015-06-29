namespace GxExpander
{
    partial class FormExpander
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
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelPaths = new System.Windows.Forms.TableLayoutPanel();
            this.lblInputPath = new System.Windows.Forms.Label();
            this.lblOutputPath = new System.Windows.Forms.Label();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.btnSelectInputPath = new System.Windows.Forms.Button();
            this.btnSelectOutputPath = new System.Windows.Forms.Button();
            this.lblGame = new System.Windows.Forms.Label();
            this.cmbGame = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanelButtonsUnpackPack = new System.Windows.Forms.TableLayoutPanel();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.groupBoxMode = new System.Windows.Forms.GroupBox();
            this.radioButtonModePack = new System.Windows.Forms.RadioButton();
            this.radioButtonModeUnpack = new System.Windows.Forms.RadioButton();
            this.folderBrowserDialogSelectInputPath = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserDialogSelectOutputPath = new System.Windows.Forms.FolderBrowserDialog();
            this.backgroundWorkerPackUnpack = new System.ComponentModel.BackgroundWorker();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelPaths.SuspendLayout();
            this.tableLayoutPanelButtonsUnpackPack.SuspendLayout();
            this.groupBoxMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelPaths, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelButtonsUnpackPack, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.groupBoxMode, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 3;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(725, 242);
            this.tableLayoutPanelMain.TabIndex = 6;
            // 
            // tableLayoutPanelPaths
            // 
            this.tableLayoutPanelPaths.ColumnCount = 3;
            this.tableLayoutPanelPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelPaths.Controls.Add(this.lblInputPath, 0, 0);
            this.tableLayoutPanelPaths.Controls.Add(this.lblOutputPath, 0, 1);
            this.tableLayoutPanelPaths.Controls.Add(this.txtInputPath, 1, 0);
            this.tableLayoutPanelPaths.Controls.Add(this.txtOutputPath, 1, 1);
            this.tableLayoutPanelPaths.Controls.Add(this.btnSelectInputPath, 2, 0);
            this.tableLayoutPanelPaths.Controls.Add(this.btnSelectOutputPath, 2, 1);
            this.tableLayoutPanelPaths.Controls.Add(this.lblGame, 0, 2);
            this.tableLayoutPanelPaths.Controls.Add(this.cmbGame, 1, 2);
            this.tableLayoutPanelPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelPaths.Location = new System.Drawing.Point(3, 99);
            this.tableLayoutPanelPaths.Name = "tableLayoutPanelPaths";
            this.tableLayoutPanelPaths.RowCount = 3;
            this.tableLayoutPanelPaths.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelPaths.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelPaths.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelPaths.Size = new System.Drawing.Size(719, 90);
            this.tableLayoutPanelPaths.TabIndex = 5;
            // 
            // lblInputPath
            // 
            this.lblInputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInputPath.AutoSize = true;
            this.lblInputPath.Location = new System.Drawing.Point(3, 0);
            this.lblInputPath.Name = "lblInputPath";
            this.lblInputPath.Size = new System.Drawing.Size(66, 30);
            this.lblInputPath.TabIndex = 0;
            this.lblInputPath.Text = "Input path:";
            // 
            // lblOutputPath
            // 
            this.lblOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOutputPath.AutoSize = true;
            this.lblOutputPath.Location = new System.Drawing.Point(3, 30);
            this.lblOutputPath.Name = "lblOutputPath";
            this.lblOutputPath.Size = new System.Drawing.Size(66, 30);
            this.lblOutputPath.TabIndex = 3;
            this.lblOutputPath.Text = "Output path:";
            // 
            // txtInputPath
            // 
            this.txtInputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputPath.Location = new System.Drawing.Point(75, 3);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(528, 20);
            this.txtInputPath.TabIndex = 1;
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputPath.Location = new System.Drawing.Point(75, 33);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(528, 20);
            this.txtOutputPath.TabIndex = 4;
            // 
            // btnSelectInputPath
            // 
            this.btnSelectInputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInputPath.Location = new System.Drawing.Point(609, 3);
            this.btnSelectInputPath.Name = "btnSelectInputPath";
            this.btnSelectInputPath.Size = new System.Drawing.Size(107, 24);
            this.btnSelectInputPath.TabIndex = 2;
            this.btnSelectInputPath.Text = "...";
            this.btnSelectInputPath.UseVisualStyleBackColor = true;
            this.btnSelectInputPath.Click += new System.EventHandler(this.btnSelectInputPath_Click);
            // 
            // btnSelectOutputPath
            // 
            this.btnSelectOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputPath.Location = new System.Drawing.Point(609, 33);
            this.btnSelectOutputPath.Name = "btnSelectOutputPath";
            this.btnSelectOutputPath.Size = new System.Drawing.Size(107, 24);
            this.btnSelectOutputPath.TabIndex = 5;
            this.btnSelectOutputPath.Text = "...";
            this.btnSelectOutputPath.UseVisualStyleBackColor = true;
            this.btnSelectOutputPath.Click += new System.EventHandler(this.btnSelectOutputPath_Click);
            // 
            // lblGame
            // 
            this.lblGame.AutoSize = true;
            this.lblGame.Location = new System.Drawing.Point(3, 60);
            this.lblGame.Name = "lblGame";
            this.lblGame.Size = new System.Drawing.Size(38, 13);
            this.lblGame.TabIndex = 6;
            this.lblGame.Text = "Game:";
            // 
            // cmbGame
            // 
            this.cmbGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGame.FormattingEnabled = true;
            this.cmbGame.Location = new System.Drawing.Point(75, 63);
            this.cmbGame.Name = "cmbGame";
            this.cmbGame.Size = new System.Drawing.Size(121, 21);
            this.cmbGame.TabIndex = 7;
            // 
            // tableLayoutPanelButtonsUnpackPack
            // 
            this.tableLayoutPanelButtonsUnpackPack.ColumnCount = 1;
            this.tableLayoutPanelButtonsUnpackPack.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButtonsUnpackPack.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelButtonsUnpackPack.Controls.Add(this.btnStartStop, 0, 0);
            this.tableLayoutPanelButtonsUnpackPack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelButtonsUnpackPack.Location = new System.Drawing.Point(3, 195);
            this.tableLayoutPanelButtonsUnpackPack.Name = "tableLayoutPanelButtonsUnpackPack";
            this.tableLayoutPanelButtonsUnpackPack.RowCount = 1;
            this.tableLayoutPanelButtonsUnpackPack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButtonsUnpackPack.Size = new System.Drawing.Size(719, 44);
            this.tableLayoutPanelButtonsUnpackPack.TabIndex = 6;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartStop.Location = new System.Drawing.Point(3, 3);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(713, 38);
            this.btnStartStop.TabIndex = 0;
            this.btnStartStop.Text = "Start/Stop";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // groupBoxMode
            // 
            this.groupBoxMode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMode.Controls.Add(this.radioButtonModePack);
            this.groupBoxMode.Controls.Add(this.radioButtonModeUnpack);
            this.groupBoxMode.Location = new System.Drawing.Point(3, 3);
            this.groupBoxMode.Name = "groupBoxMode";
            this.groupBoxMode.Size = new System.Drawing.Size(719, 90);
            this.groupBoxMode.TabIndex = 9;
            this.groupBoxMode.TabStop = false;
            this.groupBoxMode.Text = "Mode";
            // 
            // radioButtonModePack
            // 
            this.radioButtonModePack.AutoSize = true;
            this.radioButtonModePack.Location = new System.Drawing.Point(9, 42);
            this.radioButtonModePack.Name = "radioButtonModePack";
            this.radioButtonModePack.Size = new System.Drawing.Size(50, 17);
            this.radioButtonModePack.TabIndex = 1;
            this.radioButtonModePack.Text = "Pack";
            this.radioButtonModePack.UseVisualStyleBackColor = true;
            // 
            // radioButtonModeUnpack
            // 
            this.radioButtonModeUnpack.AutoSize = true;
            this.radioButtonModeUnpack.Checked = true;
            this.radioButtonModeUnpack.Location = new System.Drawing.Point(9, 19);
            this.radioButtonModeUnpack.Name = "radioButtonModeUnpack";
            this.radioButtonModeUnpack.Size = new System.Drawing.Size(63, 17);
            this.radioButtonModeUnpack.TabIndex = 0;
            this.radioButtonModeUnpack.TabStop = true;
            this.radioButtonModeUnpack.Text = "Unpack";
            this.radioButtonModeUnpack.UseVisualStyleBackColor = true;
            // 
            // backgroundWorkerPackUnpack
            // 
            this.backgroundWorkerPackUnpack.WorkerReportsProgress = true;
            this.backgroundWorkerPackUnpack.WorkerSupportsCancellation = true;
            this.backgroundWorkerPackUnpack.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerPackUnpack_DoWork);
            this.backgroundWorkerPackUnpack.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerPackUnpack_RunWorkerCompleted);
            // 
            // FormExpander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(725, 242);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormExpander";
            this.Text = "GxExpander";
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelPaths.ResumeLayout(false);
            this.tableLayoutPanelPaths.PerformLayout();
            this.tableLayoutPanelButtonsUnpackPack.ResumeLayout(false);
            this.groupBoxMode.ResumeLayout(false);
            this.groupBoxMode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPaths;
        private System.Windows.Forms.Label lblInputPath;
        private System.Windows.Forms.Label lblOutputPath;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Button btnSelectInputPath;
        private System.Windows.Forms.Button btnSelectOutputPath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtonsUnpackPack;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogSelectInputPath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogSelectOutputPath;
        private System.ComponentModel.BackgroundWorker backgroundWorkerPackUnpack;
        private System.Windows.Forms.GroupBox groupBoxMode;
        private System.Windows.Forms.RadioButton radioButtonModePack;
        private System.Windows.Forms.RadioButton radioButtonModeUnpack;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Label lblGame;
        private System.Windows.Forms.ComboBox cmbGame;

    }
}

