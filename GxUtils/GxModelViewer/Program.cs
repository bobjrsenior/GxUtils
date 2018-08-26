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
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private  const string HELP_FLAG = "-help";
        private const string INTERACTIVE_HELP_FLAG = "-interHelp";
        private const string INTERACTIVE_FLAG = "-interactive";
        private const string IMPORT_OBJ_MTL_FLAG = "-importObjMtl";
        private const string IMPORT_TPL_FLAG = "-importTPL";
        private const string IMPORT_GMA_FLAG = "-importGMA";
        private const string EXPORT_OBJ_MTL_FLAG = "-exportObjMtl";
        private const string EXPORT_TPL_FLAG = "-exportTpl";
        private const string EXPORT_GMA_FLAG = "-exportGma";

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
                bool startInteractive = HandleFlags(modelViewer, args);
                modelViewer.Dispose();
                // Place for breakpoint after flags that doesn't cause a warning
                int x = 5;
                int y = x;
                x = y;
            }
		}

        /// <summary>
        /// Handles the passed in flags. This method is used for both command line
        /// and interactive mode commands.
        /// </summary>
        /// <param name="modelViewer">ModelViewer object to apply commands on</param>
        /// <param name="flags">Array of flags to apply</param>
        /// <returns>If the caller should enter interactive mode</returns>
        private static bool HandleFlags(ModelViewer modelViewer, string[] flags)
        {
            //
            bool startInteractive = false;
            for (int i = 0; i < flags.Length; i++)
            {
                string flag = flags[i];
                switch (flag)
                {
                    case HELP_FLAG:
                        DisplayHelp();
                        break;
                    case INTERACTIVE_HELP_FLAG:

                        break;
                    case INTERACTIVE_FLAG:
                        startInteractive = true;
                        break;
                    case IMPORT_OBJ_MTL_FLAG:
                        if(i < flags.Length - 1)
                        {
                            try {
                                modelViewer.ImportObjMtl(flags[i + 1], true);
                            }
                            catch(Exception ex)
                            {
                                WriteCommandError(flag, "Error loading the OBJ file->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case IMPORT_TPL_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                modelViewer.LoadTplFile(flags[i + 1]);
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error loading the TPL file->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case IMPORT_GMA_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                modelViewer.LoadGmaFile(flags[i + 1]);
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error loading the GMA file->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case EXPORT_OBJ_MTL_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                modelViewer.ExportObjMtl(flags[i + 1]);
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error saving the OBJ/MTL file->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case EXPORT_TPL_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                modelViewer.SaveTplFile(flags[i + 1]);
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error saving the TPL file->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case EXPORT_GMA_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                modelViewer.SaveGmaFile(flags[i + 1]);
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error saving the GMA file->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    default:
                        WriteCommandError(flag, "Unknown command");
                        break;
                }
            }
            return startInteractive;
        }

        private static void DisplayHelp()
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
            Console.WriteLine("\t-interHelp\tDisplay help specific for interactive mode.");
            Console.WriteLine("\t-interactive\tStart GxModelViewer in interactive mode.");
            Console.WriteLine("\t\t\tWhile in interactive mode, GX takes newline separated commands from stdin.");
            Console.WriteLine("\t\t\tSee '-interhelp' for interactive specific commands and differences.");
            Console.WriteLine("\t-importObj <model>\t\tImports the designated .obj file.");
            Console.WriteLine("\t-importTpl <texture>\t\tImports the designated .tpl file.");
            Console.WriteLine("\t-importGma <model>\t\tImports the designated .gma file.");
            Console.WriteLine("\t-exportObjMtl <model>\t\tExports the loaded model as a .obj/.mtl file.");
            Console.WriteLine("\t-exportTpl <textures>\t\tExports the loaded textures as a .tpl file.");
            Console.WriteLine("\t-exportGma <model>\t\tExports the loaded model as a .gma file.");
        }

        private static void WriteCommandError(string command, string error)
        {
            Console.WriteLine("Invalid Command Error [" + command + "]: " + error);
        }

        private static void WriteCommandWarning(string command, string error)
        {
            Console.WriteLine("Invalid Command Warning [" + command + "]: " + error);
        }
    }
}
