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

        GcmfMaterial material;

        public MaterialFlagEditor()
        {
            InitializeComponent();
        }

        public MaterialFlagEditor(GcmfMaterial material)
        {
            InitializeComponent();
            this.material = material;
            this.flagsTextBox.Text = string.Format("{0:X8}", material.Flags);
            this.textureIndexTextBox.Text = string.Format("{0:X4}", material.TextureIdx);
            this.unknown6TextBox.Text = string.Format("{0:X2}", material.Unk6);
            this.anistropyTextBox.Text = string.Format("{0:X2}", material.AnisotropyLevel);
            this.unknownCTextBox.Text = string.Format("{0:X4}", material.UnkC);
            this.unknown10TextBox.Text = string.Format("{0:X8}", material.Unk10);
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

            if (!tryParseIntHex(this.flagsTextBox.Text, out flags)) throw new InvalidOperationException("Flags is not a valid 4 byte hex value");
            if (!tryParseShortHex(this.textureIndexTextBox.Text, out textureIndex)) throw new InvalidOperationException("Texture Index is not a valid 2 byte hex value");
            if (!tryParseByteHex(this.unknown6TextBox.Text, out unk6)) throw new InvalidOperationException("Unknown 6 is not a valid 1 byte hex value");
            if (!tryParseByteHex(this.anistropyTextBox.Text, out anisotropy)) throw new InvalidOperationException("Anisotropy is not a valid 1 byte hex value");
            if (!tryParseShortHex(this.unknownCTextBox.Text, out unkC)) throw new InvalidOperationException("Unknown C is not a valid 2 byte hex value");
            if (!tryParseIntHex(this.unknown10TextBox.Text, out unk10)) throw new InvalidOperationException("Unknown 10 is not a valid 4 byte hex value");

            material.Flags = flags;
            material.TextureIdx = textureIndex;
            material.Unk6 = unk6;
            material.AnisotropyLevel = anisotropy;
            material.UnkC = unkC;
            material.Unk10 = unk10;
        }

        private bool tryParseByteHex(string hex, out byte hexValue)
        {
            try
            {
                hexValue = Convert.ToByte(hex, 16);
                return true;
            }
            catch (Exception ex)
            {
                hexValue = 0;
                return false;
            }
        }

        private bool tryParseShortHex(string hex, out ushort hexValue)
        {
            try
            {
                hexValue = Convert.ToUInt16(hex, 16);
                return true;
            }
            catch (Exception ex)
            {
                hexValue = 0;
                return false;
            }
        }

        private bool tryParseIntHex(string hex, out uint hexValue)
        {
            try
            {
                hexValue = Convert.ToUInt32(hex, 16);
                return true;
            }
            catch (Exception ex)
            {
                hexValue = 0;
                return false;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Request image filename
            if (saveFlagsFileDialog.ShowDialog() != DialogResult.OK)
                return;

            StringBuilder sb = new StringBuilder()
                .Append(FLAGS).Append(" ").Append(this.flagsTextBox.Text).Append("\r\n")
                .Append(TEXTURE_INDEX).Append(" ").Append(this.textureIndexTextBox.Text).Append("\r\n")
                .Append(UNKNOWN_6).Append(" ").Append(this.unknown6TextBox.Text).Append("\r\n")
                .Append(ANISOTROPY).Append(" ").Append(this.anistropyTextBox.Text).Append("\r\n")
                .Append(UNKNOWN_C).Append(" ").Append(this.unknownCTextBox.Text).Append("\r\n")
                .Append(UNKNOWN_10).Append(" ").Append(this.unknown10TextBox.Text);

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
