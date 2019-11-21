using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibGxTexture;
using LibGxFormat;
using System.Text.RegularExpressions;

namespace GxModelViewer
{
    public partial class AddTextureHeader : Form
    {
        private int fileSize;
        private GxTextureFormat currentFormat;

        public AddTextureHeader(IEnumerable<GxTextureFormat> availableFormats, GxTextureFormat defaultFormat, String fileName, int fileSize)
        {
            this.fileSize = fileSize;
            if (availableFormats == null)
                throw new ArgumentNullException("availableFormats");
            if (!availableFormats.Contains(defaultFormat))
                throw new ArgumentOutOfRangeException("defaultFormat", "Default format not in list of available formats.");

            InitializeComponent();

            // Populate the format combobox from the available formats
            cmbFormat.ValueMember = "Key";
            cmbFormat.DisplayMember = "Value";
            cmbFormat.DataSource = new BindingSource(availableFormats
                .Select(g => new { Key = g, Value = string.Format("{0} ({1})", g, EnumUtils.GetEnumDescription(g)) }).ToArray(), null);

            // Select the default format given
            cmbFormat.SelectedValue = defaultFormat;
            
            // Obtains dimensions from filename
            Match defaultValues = new Regex(@"(\d+)x(\d+)").Match(fileName);

            // Automatically fills in dimensions if they exist
            if (defaultValues.Groups[1].Value != null || defaultValues.Groups[2].Value != null)
            {
                texX.Text = defaultValues.Groups[1].Value;
                texY.Text = defaultValues.Groups[2].Value;
            }

            // Otherwise, they are set to zero
            else
            {
                texX.Text = "0";
                texY.Text = "0";
            }

            // Most headerless TPLs seem to only have one mipmap level
            texMipmapCount.Text = "1";

            // Obtains texture count from filesize
            updateTextureCount();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public GeneratedTextureHeader getTextureHeader()
        {
            GeneratedTextureHeader returnTexture = new GeneratedTextureHeader();
            returnTexture.textureFormat = (GxTextureFormat)cmbFormat.SelectedValue;
            returnTexture.textureCount = Convert.ToInt32(texCount.Text);
            returnTexture.textureWidth = Convert.ToInt32(texX.Text);
            returnTexture.textureHeight = Convert.ToInt32(texY.Text);
            returnTexture.textureMipmapCount = Convert.ToInt32(texMipmapCount.Text);
            return returnTexture;
        }

        private void updateTextureCount()
        {
            int.TryParse(texX.Text, out int x);
            int.TryParse(texY.Text, out int y);
            int texSize = x * y * (GxTextureFormatCodec.GetCodec(currentFormat).BitsPerPixel / 8);
            if (texSize != 0)
            {
                int finalCount = fileSize / texSize;
                texCount.Text = finalCount.ToString();
            }
        }

        private void texX_Leave(object sender, EventArgs e)
        {
            if (int.TryParse(texX.Text, out int x))
            {
                updateTextureCount();
            }
        }

        private void texY_Leave(object sender, EventArgs e)
        {
            if (int.TryParse(texY.Text, out int y))
            {
                updateTextureCount();
            }
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentFormat = (GxTextureFormat)cmbFormat.SelectedValue;
            updateTextureCount();
        }
    }
}
