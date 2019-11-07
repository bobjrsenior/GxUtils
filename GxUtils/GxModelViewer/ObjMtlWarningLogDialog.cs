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
    /// Dialog used to show a list of warnings to the user after loading flags.
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
