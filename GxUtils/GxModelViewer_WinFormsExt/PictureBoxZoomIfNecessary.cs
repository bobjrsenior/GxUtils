using System;
using System.Windows.Forms;

namespace GxModelViewer_WinFormsExt
{
    /// <summary>
    /// A PictureBox which will show the Image always centered on its area.
    /// If the image is too big to fit, it will be downsized while keeping the aspect ratio.
    /// </summary>
    public class PictureBoxDownsizeIfNecessary : PictureBox
    {
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image != null)
            {
                if (Image.Width > Width || Image.Height > Height)
                {
                    // Image too big -> Use Zoom mode which will scale and center the image keeping aspect ratio
                    SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    // Image fits -> Use CenterImage mode which will center the image without resizing
                    SizeMode = PictureBoxSizeMode.CenterImage;
                }
            }

            base.OnPaint(pe);
        }
    }
}
