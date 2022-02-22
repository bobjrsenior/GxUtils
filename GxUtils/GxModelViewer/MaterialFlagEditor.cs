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
    public partial class MaterialFlagEditor : Form
    {
        private const string FLAGS = "FLAGS";
        private const string TEXTURE_INDEX = "TEXTURE_INDEX";
        private const string UNKNOWN_6 = "UNKNOWN_6";
        private const string ANISOTROPY = "ANISOTROPY";
        private const string UNKNOWN_C = "UNKNOWN_C";
        private const string UNKNOWN_10 = "UNKNOWN_10";

        List<GcmfMaterial> materials;

        public MaterialFlagEditor()
        {
            InitializeComponent();
        }


        public MaterialFlagEditor(List<GcmfMaterial> materials)
        {
            InitializeComponent();
            this.materials = materials;
            this.flagsTextBox.Text = string.Format("{0:X8}", materials[0].Flags);
            this.textureIndexTextBox.Text = string.Format("{0:X4}", materials[0].TextureIdx);
            this.unknown6TextBox.Text = string.Format("{0:X2}", materials[0].Unk6);
            this.anistropyTextBox.Text = string.Format("{0:X2}", materials[0].AnisotropyLevel);
            this.unknownCTextBox.Text = string.Format("{0:X4}", materials[0].UnkC);
            this.unknown10TextBox.Text = string.Format("{0:X8}", materials[0].Unk10);

            for(int i = 1; i < materials.Count; i++)
            {
                if (materials[i].Flags != materials[0].Flags) this.flagsTextBox.Text = FlagHelper.ERROR_VALUE;
                if (materials[i].TextureIdx != materials[0].TextureIdx) this.textureIndexTextBox.Text = FlagHelper.ERROR_VALUE;
                if (materials[i].Unk6 != materials[0].Unk6) this.unknown6TextBox.Text = FlagHelper.ERROR_VALUE;
                if (materials[i].AnisotropyLevel != materials[0].AnisotropyLevel) this.flagsTextBox.Text = FlagHelper.ERROR_VALUE;
                if (materials[i].UnkC != materials[0].UnkC) this.unknownCTextBox.Text = FlagHelper.ERROR_VALUE;
                if (materials[i].Unk10 != materials[0].Unk10) this.unknown10TextBox.Text = FlagHelper.ERROR_VALUE;
            }
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            try
            {
                validateInput();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("The flags could not be updated: " + ex.Message,
                    "Error updating flags.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void validateInput()
        {
            byte unk6, anisotropy;
            ushort textureIndex, unkC;
            uint flags, unk10;
            bool unk6Valid, anisotropyValid, textureIndexValid, unkCValid, flagsValid, unk10Valid;

            flagsValid = FlagHelper.parseHexToInt32(this.flagsTextBox.Text, out flags, "Flags is not a valid 4 byte hex value");
            textureIndexValid = FlagHelper.parseHexToShort(this.textureIndexTextBox.Text, out textureIndex, "Texture Index is not a valid 2 byte hex value");
            unk6Valid = FlagHelper.parseHexToByte(this.unknown6TextBox.Text, out unk6, "Unknown 6 is not a valid 1 byte hex value");
            anisotropyValid = FlagHelper.parseHexToByte(this.anistropyTextBox.Text, out anisotropy, "Anisotropy is not a valid 1 byte hex value");
            unkCValid = FlagHelper.parseHexToShort(this.unknownCTextBox.Text, out unkC, "Unknown C is not a valid 2 byte hex value");
            unk10Valid = FlagHelper.parseHexToInt32(this.unknown10TextBox.Text, out unk10, "Unknown 10 is not a valid 4 byte hex value");

            foreach (GcmfMaterial material in materials)
            {
                if(flagsValid) material.Flags = flags;
                if(textureIndexValid) material.TextureIdx = textureIndex;
                if(unk6Valid) material.Unk6 = unk6;
                if(anisotropyValid) material.AnisotropyLevel = anisotropy;
                if(unkCValid) material.UnkC = unkC;
                if(unk10Valid) material.Unk10 = unk10;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Request image filename
            if (saveFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;

            StringBuilder sb = new StringBuilder();
            if (this.flagsTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(FLAGS).Append(" ").Append(this.flagsTextBox.Text).Append("\r\n");
            if (this.textureIndexTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(TEXTURE_INDEX).Append(" ").Append(this.textureIndexTextBox.Text).Append("\r\n");
            if (this.unknown6TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_6).Append(" ").Append(this.unknown6TextBox.Text).Append("\r\n");
            if (this.anistropyTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(ANISOTROPY).Append(" ").Append(this.anistropyTextBox.Text).Append("\r\n");
            if (this.unknownCTextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_C).Append(" ").Append(this.unknownCTextBox.Text).Append("\r\n");
            if (this.unknown10TextBox.Text != FlagHelper.ERROR_VALUE) sb.Append(UNKNOWN_10).Append(" ").Append(this.unknown10TextBox.Text).Append("\r\n");

            System.IO.File.WriteAllText(saveFlagsFileDialog.FileName, sb.ToString());
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            // Request image filename
            if (openFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(openFlagsFileDialog.FileName);

                List<string> flagWarningLog = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] line = lines[i].Split();
                    if (line.Length == 2)
                    {
                        switch (line[0])
                        {
                            case FLAGS:
                                this.flagsTextBox.Text = line[1];
                                break;
                            case TEXTURE_INDEX:
                                this.textureIndexTextBox.Text = line[1];
                                break;
                            case UNKNOWN_6:
                                this.unknown6TextBox.Text = line[1];
                                break;
                            case ANISOTROPY:
                                this.anistropyTextBox.Text = line[1];
                                break;
                            case UNKNOWN_C:
                                this.unknownCTextBox.Text = line[1];
                                break;
                            case UNKNOWN_10:
                                this.unknown10TextBox.Text = line[1];
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
                    FlagsWarningLogDialog warningDlg = new FlagsWarningLogDialog(flagWarningLog, "Material Flag Import Warnings", "The following warnings were issued while importing the material flags:");
                    if (warningDlg.ShowDialog() != DialogResult.Yes)
                        return;
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
