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
    /// <summary>
    /// Dialog used to show a list of warnings to the user after loading a .OBJ/.MTL file,
    /// asking him if he wishes to continue.
    /// Sets DialogResult to Yes/No according to the answer of the user.
    /// Note that DialogResult may be set to Cancel if the user closes the dialog box with the close button.
    /// </summary>
    public partial class ObjMtlWarningLogDialog : Form
    {
        public ObjMtlWarningLogDialog(List<string> warningLog)
        {
            if (warningLog == null)
                throw new ArgumentNullException("warningLog");

            InitializeComponent();

            tbWarningList.Text = string.Join("\r\n", warningLog);
        }
    }
}
