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

        private const string HELP_FLAG = "-help";
        private const string INTERACTIVE_HELP_FLAG = "-interHelp";
        private const string INTERACTIVE_FLAG = "-interactive";
        private const string GAME_FLAG = "-game";
        private const string MIPMAPS_FLAG = "-mipmaps";
        private const string INTERPOLATION_FLAG = "-interpolate";
        private const string IMPORT_OBJ_MTL_FLAG = "-importObjMtl";
        private const string IMPORT_TPL_FLAG = "-importTpl";
        private const string IMPORT_GMA_FLAG = "-importGma";
        private const string MERGE_GMATPL_FLAG = "-mergeGmaTpl";
        private const string EXPORT_OBJ_MTL_FLAG = "-exportObjMtl";
        private const string EXPORT_TPL_FLAG = "-exportTpl";
        private const string EXPORT_GMA_FLAG = "-exportGma";
        private const string FIX_SCROLLING_TEXTURES = "-fixScrollingTextures";
        private const string FIX_TRANSPARENCY = "-fixTransparentMeshes";
        private const string SET_ALL_MIPMAPS = "-setAllMipmaps";
        private const string REMOVE_UNUSED_TEXTURES = "-removeUnusedTextures";
        private const string PRESET_FOLDER_FLAG = "-setPresetFolder";

        // Interactive Mode Only
        private const string QUIT_FLAG = "-quit";

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
                bool startInteractive = HandleFlags(modelViewer, args, false);
                if (startInteractive)
                {
                    InteractiveMode(modelViewer);
                }
                modelViewer.Dispose();
             }
		}

        private static void InteractiveMode(ModelViewer modelViewer)
        {
            bool continueReading = true;
            do
            {
                string line = Console.ReadLine();
                string[] args = SplitLineToArgs(line);
                continueReading = HandleFlags(modelViewer, args, true);

            } while (continueReading);
        }

        /// <summary>
        /// Handles the passed in flags. This method is used for both command line
        /// and interactive mode commands.
        /// </summary>
        /// <param name="modelViewer">ModelViewer object to apply commands on</param>
        /// <param name="flags">Array of flags to apply</param>
        /// <param name="isInteractive">Determines if currently in interactive mode</param>
        /// <returns>If the caller should enter interactive mode. If in interactive mode, this determines if input should continue.</returns>
        private static bool HandleFlags(ModelViewer modelViewer, string[] flags, bool isInteractive)
        {
            bool startInteractive = false;
            for (int i = 0; i < flags.Length; i++)
            {
                string flag = flags[i];
                switch (flag)
                {
                    case HELP_FLAG:
                        DisplayHelp();
                        WriteCommandSuccess(flag);
                        break;
                    case INTERACTIVE_HELP_FLAG:
                        DisplayInteractiveHelp();
                        WriteCommandSuccess(flag);
                        break;
                    case INTERACTIVE_FLAG:
                        startInteractive = true;
                        if (!isInteractive)
                            WriteCommandSuccess(flag);
                        break;
                    case QUIT_FLAG:
                        if (isInteractive)
                        {
                            WriteCommandSuccess(flag);
                            return false;
                        }
                        break;
                    case GAME_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                string gameOrig = flags[i + 1];
                                bool valid = false;
                                LibGxFormat.GxGame game = LibGxFormat.GxGame.SuperMonkeyBall;
                                switch (gameOrig)
                                {
                                    case "smb":
                                        game = LibGxFormat.GxGame.SuperMonkeyBall;
                                        valid = true;
                                        break;
                                    case "deluxe":
                                        game = LibGxFormat.GxGame.SuperMonkeyBallDX;
                                        valid = true;
                                        break;
                                    case "fzero":
                                        game = LibGxFormat.GxGame.FZeroGX;
                                        valid = true;
                                        break;
                                }
                                if (valid)
                                {
                                    modelViewer.SetSelectedGame(game);
                                    WriteCommandSuccess(flag);
                                }
                                else
                                {
                                    WriteCommandError(flag, "Game selected does not exist->" + gameOrig);
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error setting game->" + ex.Message);
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
                    case MIPMAPS_FLAG:
                        if (i < flags.Length - 1)
                        {
                            int value;
                            bool validInt = int.TryParse(flags[i + 1], out value);
                            if (validInt && value >= 0)
                            {
                                modelViewer.SetNumMipmaps(value);
                                WriteCommandSuccess(flag);
                            }
                            else
                            {
                                WriteCommandError(flag, "Value is not a valid (positive/zero) int->" + flags[i + 1]);
                            }
                            i++;
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case INTERPOLATION_FLAG:
                        if (i < flags.Length - 1)
                        {
                            string interpolateString = flags[i + 1];
                            bool valid = false;
                            LibGxFormat.GxInterpolationFormat format = LibGxFormat.GxInterpolationFormat.HighQualityBicubic;
                            switch (interpolateString)
                            {
                                case "bicubic":
                                    format = LibGxFormat.GxInterpolationFormat.HighQualityBicubic;
                                    valid = true;
                                    break;
                                case "nearest":
                                case "nn":
                                    format = LibGxFormat.GxInterpolationFormat.NearestNeighbor;
                                    valid = true;
                                    break;
                                case "csharpdefault":
                                    format = LibGxFormat.GxInterpolationFormat.CSharpDefault;
                                    valid = true;
                                    break;
                            }
                            if (valid)
                            {
                                modelViewer.SetSelectedMipmap(format);
                                WriteCommandSuccess(flag);
                            }
                            else
                            {
                                WriteCommandError(flag, "Value is not a valid interpolation type->" + interpolateString);
                            }
                            i++;
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case FIX_SCROLLING_TEXTURES:
                        try
                        {
                            modelViewer.FixScrollingTextures();
                            WriteCommandSuccess(flag);
                        }
                        catch (Exception ex)
                        {
                            WriteCommandError(flag, "Error updating flags for scrollable textures->" + ex.Message + "\n" + ex.StackTrace);
                        }
                        break;
                    case FIX_TRANSPARENCY:
                        try
                        {
                            modelViewer.FixTransparency();
                            WriteCommandSuccess(flag);
                        }
                        catch (Exception ex)
                        {
                            WriteCommandError(flag, "Error updating flags for transparent meshes->" + ex.Message + "\n" + ex.StackTrace);
                        }
                        break;
                    case IMPORT_OBJ_MTL_FLAG:
                        if(i < flags.Length - 1)
                        {
                            try {
                                modelViewer.ImportObjMtl(flags[i + 1], true);
                                WriteCommandSuccess(flag);
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
                                WriteCommandSuccess(flag);
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
                                WriteCommandSuccess(flag);
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
                    case MERGE_GMATPL_FLAG:
                        if (i < flags.Length - 1 && flags[i + 1].Split(',').Length == 2)
                        {
                            try
                            {
                                string newgmapath = flags[i + 1].Split(',')[0];
                                string newtplpath = flags[i + 1].Split(',')[1];
                                modelViewer.MergeGMATPLFiles(newgmapath,newtplpath);
                                WriteCommandSuccess(flag);
                            }
                            catch (Exception ex)
                            {
                                WriteCommandError(flag, "Error merging in the file GMA and TPL files->" + ex.Message);
                            }
                            finally
                            {
                                // Skip the command argument
                                i++;
                            }
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command, provide GMA filepath and TPL filepath separated by comma (Ex: dir1/file.gma,dir2/file.tpl)");
                        }
                        break;
                    case EXPORT_OBJ_MTL_FLAG:
                        if (i < flags.Length - 1)
                        {
                            try
                            {
                                modelViewer.ExportObjMtl(flags[i + 1]);
                                WriteCommandSuccess(flag);
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
                                WriteCommandSuccess(flag);
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
                                WriteCommandSuccess(flag);
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
                    case SET_ALL_MIPMAPS:
                        if (i < flags.Length - 1)
                        {
                            int value;
                            bool validInt = int.TryParse(flags[i + 1], out value);
                            if (validInt && value >= 0)
                            {
                                try
                                {
                                    modelViewer.setAllMipmaps(value);
                                    WriteCommandSuccess(flag);
                                }
                                catch (Exception ex)
                                {
                                    WriteCommandError(flag, "Error setting mipmap values->" + ex.Message);
                                }
                            }
                            else
                            {
                                WriteCommandError(flag, "Value is not a valid (positive/zero) int->" + flags[i + 1]);
                            }
                            i++;
                        }
                        else
                        {
                            WriteCommandError(flag, "Not enough args for command");
                        }
                        break;
                    case REMOVE_UNUSED_TEXTURES:
                        try
                        {
                            modelViewer.DeleteUnusedTextures();
                            WriteCommandSuccess(flag);
                        }
                        catch (Exception ex)
                        {
                            WriteCommandError(flag, "Error removing unused textures->" + ex.Message + "\n" + ex.StackTrace);
                        }
                        break;

                    case PRESET_FOLDER_FLAG:
                        if (i < flags.Length - 1)
                        {
                            bool exists = Directory.Exists(flags[i + 1]);
                            if (exists)
                            {
                                modelViewer.presetFolder = flags[i + 1];
                                WriteCommandSuccess(flag);
                            }
                            else
                            {
                                WriteCommandError(flag, "Directory does not exist->" + flags[i+1]);
                            }

                            i++;
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

            // Interactive mode returns false to continue reading, true to quit
            if (isInteractive)
            {
                return true;
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
            Console.WriteLine("\t-help\t\t\t\tDisplay this help.");
            Console.WriteLine("\t-interHelp\t\t\tDisplay help specific for interactive mode.");
            Console.WriteLine("\t-interactive\t\t\tStart GxModelViewer in interactive mode.");
            Console.WriteLine("\t\t\t\t\tWhile in interactive mode, GX takes newline separated commands from stdin.");
            Console.WriteLine("\t\t\t\t\tSee '-interhelp' for interactive specific commands and differences.");
            Console.WriteLine("\t-game <type>\t\t\tThe game to use when importing/exporting.");
            Console.WriteLine("\t\t\t\t\t\tsmb: Super Monkey Ball 1/2 (default)");
            Console.WriteLine("\t\t\t\t\t\tdeluxe: Super Monkey Ball Deluxe (beta)");
            Console.WriteLine("\t\t\t\t\t\tfzero: F-Zero GX");
            Console.WriteLine("\t-mipmaps <num>\t\t\tThe number of mipmaps to make on import.");
            Console.WriteLine("\t-interpolate <type>\t\tThe type of interpolation to use with mipmap generation.");
            Console.WriteLine("\t\t\t\t\t\tbicubic: High quality bicubic (default)");
            Console.WriteLine("\t\t\t\t\t\tcsharpdefault: The C# default type (bilinear)");
            Console.WriteLine("\t\t\t\t\t\tnearest: Nearest neighbor");           
            Console.WriteLine("\t\t\t\t\t\tnn: Nearest Neighbor alias");
            Console.WriteLine("\t-importObjMtl <model>\t\tImports the designated .obj file.");
            Console.WriteLine("\t-importTpl <texture>\t\tImports the designated .tpl file.");
            Console.WriteLine("\t-importGma <model>\t\tImports the designated .gma file.");
            Console.WriteLine("\t-exportObjMtl <model>\t\tExports the loaded model as a .obj/.mtl file.");
            Console.WriteLine("\t-exportTpl <textures>\t\tExports the loaded textures as a .tpl file.");
            Console.WriteLine("\t-exportGma <model>\t\tExports the loaded model as a .gma file.");
            Console.WriteLine("\t-setAllMipmaps <num>\t\tSets the number of mipmaps for every loaded texture to <num>.");
            Console.WriteLine("\t\t\t\t\tTexture files should be loaded before calling this flag.");
            Console.WriteLine("\t-mergeGmaTpl <GMA>,<TPL>\t\tMerges the specified GMA and TPL with the active GMA and TPL.");
            Console.WriteLine("\t-fixScrollingTextures\t\tSets texture scroll flag for all materials of a model with 'texture'");
            Console.WriteLine("\t\t\t\t\tand 'scroll' in the name.");
            Console.WriteLine("\t-fixTransparentMeshes\t\tSets transparency on all models with 'transparency100%' or 'transparent100%'");
            Console.WriteLine("\t\t\t\t\tin their name, or any variant in steps of 25% (0%, 25%, 50%, 75%, 100%)");
            Console.WriteLine("\t-removeUnusedTextures\t\tRemoves textures that are not used by any materials.");
            Console.WriteLine("\t-setPresetFolder\t\tSets the folder to look for preset files in.");

        }

        private static void DisplayInteractiveHelp()
        {
            Console.WriteLine("GX Model Viewer Interactive Mode Help");
            Console.WriteLine("Description:");
            Console.WriteLine("\tGX Model Viewer Interactive mode works by reading lines from standard input.");
            Console.WriteLine("\tEach line of input works just like command line arguments, so each line can have multiple commands.");
            Console.WriteLine("\tInput continues to be read until the -quit command is recieved.");
            Console.WriteLine("\tThe -interactive command does NOT do anything in interactive mode.");
            Console.WriteLine("");
            Console.WriteLine("Special Interactive Mode Args:");
            Console.WriteLine("\t-quit\t\t\t\tQuit interactive mode and exit the program.");
   
        }

        private static void WriteCommandSuccess(string command)
        {
            WriteCommandInfo(command, "Command completed successfully");
        }

        private static void WriteCommandError(string command, string error)
        {
            Console.WriteLine("Invalid Command Error [" + command + "]: " + error);
        }

        private static void WriteCommandWarning(string command, string warning)
        {
            Console.WriteLine("Invalid Command Warning [" + command + "]: " + warning);
        }

        private static void WriteCommandInfo(string command, string info)
        {
            Console.WriteLine("Info [" + command + "]: " + info);
        }

        /// <summary>
        /// Takes an input line and splits it into arguments. Use quotes to allow spaces
        /// in an argument.
        /// </summary>
        /// <param name="line">Line to parse</param>
        /// <returns>Parsed arguments</returns>
        private static string[] SplitLineToArgs(string line)
        {
            // Takes the input line and replaces spaces with newline characters
            // unless the spaces are within quotes.
            // Note: This means quotes can be used as argument values
            char[] separators = new char[] { '\n' };
            char[] characters = line.ToCharArray();
            bool inQuote = false;
            for(int i = 0; i < characters.Length; i++)
            {
                if(characters[i] == '"')
                {
                    inQuote = !inQuote;
                    characters[i] = '\n';
                }
                if(!inQuote && characters[i] == ' ')
                {
                    characters[i] = '\n';
                }
            }
            string newLine = new string(characters);
            return newLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
