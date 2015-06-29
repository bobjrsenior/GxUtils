using System;
using System.Windows.Forms;

namespace GxModelViewer
{
	class MainClass
	{
        [STAThread]
		public static void Main (string[] args)
		{
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModelViewer());
		}
	}
}
