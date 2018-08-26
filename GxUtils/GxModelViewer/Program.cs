using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LibGxFormat.ModelLoader;
using System.IO;
using System.Collections.Generic;

namespace GxModelViewer
{
	class MainClass
	{
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [STAThread]
		public static void Main (string[] args)
		{
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ModelViewer modelViewer = new ModelViewer();
            
            if (args.Length == 0)
            {
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);
                Application.Run(modelViewer);
            }
            else
            {
                displayHelp();
            }
		}

        public static void displayHelp()
        {
            Console.WriteLine("GX Model Viewer Command Line Help");
            Console.WriteLine("Description:");
            Console.WriteLine("\tIf no arguments are given, GxModelViewer starts in its normal GUI mode.");
            Console.WriteLine("\tOtherwise the GUI will not open and it becomes command line only.");
            Console.WriteLine("\tSee the '-interactive' switch for interactive mode.");
            Console.WriteLine(@"Usage: .\GxModelViewer [arg [value...]...]");
            Console.WriteLine("");
            Console.WriteLine("args:");
            Console.WriteLine("\t-help\t\tDisplay this help.");
            Console.WriteLine("\t-interhelp\tDisplay help specific for interactive mode.");
            Console.WriteLine("\t-interactive\tStart GxModelViewer in interactive mode.");
            Console.WriteLine("\t\t\tWhile in interactive mode, GX takes newline separated commands from stdin.");
            Console.WriteLine("\t\t\tSee '-interhelp' for interactive specific commands and differences.");
        }
	}
}
