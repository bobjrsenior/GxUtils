namespace GxModelViewer
{
    partial class MaterialFlagEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.flagsTextBox = new System.Windows.Forms.TextBox();
            this.textureIndexTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.anistropyTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.unknownCTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.unknown10TextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.okayButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.unknown6TextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.loadButton = new System.Windows.Forms.Button();
            this.saveFlagsFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFlagsFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Flags (0x00)";
            // 
            // flagsTextBox
            // 
            this.flagsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flagsTextBox.Location = new System.Drawing.Point(150, 3);
            this.flagsTextBox.MaxLength = 8;
            this.flagsTextBox.Name = "flagsTextBox";
            this.flagsTextBox.Size = new System.Drawing.Size(100, 20);
            this.flagsTextBox.TabIndex = 0;
            // 
            // textureIndexTextBox
            // 
            this.textureIndexTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textureIndexTextBox.Location = new System.Drawing.Point(150, 29);
            this.textureIndexTextBox.MaxLength = 4;
            this.textureIndexTextBox.Name = "textureIndexTextBox";
            this.textureIndexTextBox.Size = new System.Drawing.Size(100, 20);
            this.textureIndexTextBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(126, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "0x";
            // 
            // anistropyTextBox
            // 
            this.anistropyTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.anistropyTextBox.Location = new System.Drawing.Point(150, 81);
            this.anistropyTextBox.MaxLength = 2;
            this.anistropyTextBox.Name = "anistropyTextBox";
            this.anistropyTextBox.Size = new System.Drawing.Size(100, 20);
            this.anistropyTextBox.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Anisotropy Level (0x07)";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Texture Index (0x04)";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(126, 110);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "0x";
            // 
            // unknownCTextBox
            // 
            this.unknownCTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.unknownCTextBox.Location = new System.Drawing.Point(150, 107);
            this.unknownCTextBox.MaxLength = 4;
            this.unknownCTextBox.Name = "unknownCTextBox";
            this.unknownCTextBox.Size = new System.Drawing.Size(100, 20);
            this.unknownCTextBox.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 110);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Unknown (0x0C)";
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(126, 138);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "0x";
            // 
            // unknown10TextBox
            // 
            this.unknown10TextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.unknown10TextBox.Location = new System.Drawing.Point(150, 135);
            this.unknown10TextBox.MaxLength = 8;
            this.unknown10TextBox.Name = "unknown10TextBox";
            this.unknown10TextBox.Size = new System.Drawing.Size(100, 20);
            this.unknown10TextBox.TabIndex = 5;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 138);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(85, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Unknown (0x10)";
            // 
            // okayButton
            // 
            this.okayButton.Location = new System.Drawing.Point(51, 222);
            this.okayButton.Name = "okayButton";
            this.okayButton.Size = new System.Drawing.Size(75, 23);
            this.okayButton.TabIndex = 6;
            this.okayButton.Text = "OK";
            this.okayButton.UseVisualStyleBackColor = true;
            this.okayButton.Click += new System.EventHandler(this.okayButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(142, 222);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(126, 58);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(18, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "0x";
            // 
            // unknown6TextBox
            // 
            this.unknown6TextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.unknown6TextBox.Location = new System.Drawing.Point(150, 55);
            this.unknown6TextBox.MaxLength = 2;
            this.unknown6TextBox.Name = "unknown6TextBox";
            this.unknown6TextBox.Size = new System.Drawing.Size(100, 20);
            this.unknown6TextBox.TabIndex = 2;
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 58);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 13);
            this.label11.TabIndex = 17;
            this.label11.Text = "Unknown (0x06)";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(51, 193);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 8;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(142, 193);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 23);
            this.loadButton.TabIndex = 9;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // saveFlagsFileDialog
            // 
            this.saveFlagsFileDialog.DefaultExt = "txt";
            this.saveFlagsFileDialog.FileName = "MaterialFlags";
            this.saveFlagsFileDialog.Filter = "Text Files|*.txt|All FIles|*.*";
            // 
            // openFlagsFileDialog
            // 
            this.openFlagsFileDialog.DefaultExt = "txt";
            this.openFlagsFileDialog.FileName = "MaterialFlags";
            this.openFlagsFileDialog.Filter = "Text Files|*.txt|All FIles|*.*";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.unknown6TextBox, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label10, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label11, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.textureIndexTextBox, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flagsTextBox, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.anistropyTextBox, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.unknownCTextBox, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.unknown10TextBox, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label12, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 13);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(255, 160);
            this.tableLayoutPanel1.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(126, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "0x";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(126, 6);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(18, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "0x";
            // 
            // MaterialFlagEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 256);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okayButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MaterialFlagEditor";
            this.Text = "Material Flag Editor";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox flagsTextBox;
        private System.Windows.Forms.TextBox textureIndexTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox anistropyTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox unknownCTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox unknown10TextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button okayButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox unknown6TextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.SaveFileDialog saveFlagsFileDialog;
        private System.Windows.Forms.OpenFileDialog openFlagsFileDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label2;
    }
}