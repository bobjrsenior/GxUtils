namespace GxModelViewer
{
    partial class GxTextureFormatPickerDialog
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
            this.lblPickFormatInfoText = new System.Windows.Forms.Label();
            this.cmbFormat = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lblFormat = new System.Windows.Forms.Label();
            this.tableLayoutPanelLayout = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanelLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPickFormatInfoText
            // 
            this.lblPickFormatInfoText.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPickFormatInfoText.AutoSize = true;
            this.tableLayoutPanelLayout.SetColumnSpan(this.lblPickFormatInfoText, 2);
            this.lblPickFormatInfoText.Location = new System.Drawing.Point(3, 0);
            this.lblPickFormatInfoText.Name = "lblPickFormatInfoText";
            this.lblPickFormatInfoText.Size = new System.Drawing.Size(274, 13);
            this.lblPickFormatInfoText.TabIndex = 0;
            this.lblPickFormatInfoText.Text = "Select the texture format to use (if unsure, don\'t change):";
            // 
            // cmbFormat
            // 
            this.cmbFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(86, 21);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(327, 21);
            this.cmbFormat.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnSubmit);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(86, 53);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(327, 25);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(249, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSubmit
            // 
            this.btnSubmit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSubmit.Location = new System.Drawing.Point(168, 3);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 0;
            this.btnSubmit.Text = "Select";
            this.btnSubmit.UseVisualStyleBackColor = true;
            // 
            // lblFormat
            // 
            this.lblFormat.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(38, 25);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(42, 13);
            this.lblFormat.TabIndex = 1;
            this.lblFormat.Text = "Format:";
            // 
            // tableLayoutPanelLayout
            // 
            this.tableLayoutPanelLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelLayout.ColumnCount = 2;
            this.tableLayoutPanelLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanelLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanelLayout.Controls.Add(this.flowLayoutPanel1, 1, 2);
            this.tableLayoutPanelLayout.Controls.Add(this.lblPickFormatInfoText, 0, 0);
            this.tableLayoutPanelLayout.Controls.Add(this.cmbFormat, 1, 1);
            this.tableLayoutPanelLayout.Controls.Add(this.lblFormat, 0, 1);
            this.tableLayoutPanelLayout.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanelLayout.Name = "tableLayoutPanelLayout";
            this.tableLayoutPanelLayout.RowCount = 3;
            this.tableLayoutPanelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelLayout.Size = new System.Drawing.Size(416, 81);
            this.tableLayoutPanelLayout.TabIndex = 4;
            // 
            // GxTextureFormatPickerDialog
            // 
            this.AcceptButton = this.btnSubmit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(440, 105);
            this.Controls.Add(this.tableLayoutPanelLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "GxTextureFormatPickerDialog";
            this.Text = "Texture Format Selection";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanelLayout.ResumeLayout(false);
            this.tableLayoutPanelLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPickFormatInfoText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLayout;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.Label lblFormat;
    }
}