using System;
using System.ComponentModel;
using System.IO;
using LibGxFormat;
using LibGxFormat.Arc;
using LibGxFormat.Lz;

namespace GxExpander
{
    /* About extensions and comma-extensions
     * -------------------------------------
     * The objective of this program is to unpack and pack F-Zero GX content files.
     *
     * Of course, to be able to pack everything again,
     * we need to know which packing should be applied to each file.
     *
     * It'd possible to use a list of files, but that approach is unflexible
     * (needs a separate file, not easy to edit, needs a parser, etc.).
     *
     * In order to solve this, I introduce COMMA-EXTENSIONS.
     * They are like a extension, but with a comma instead of a dot.
     * They are located between the file name, and the normal extensions.
     * They specify how to pack a file.
     *
     * If a file is unpacked, its extensions are changed to a comma-extension.
     * For example:
     * xxx.lz -> xxx,lz
     * xxx.yyy.lz -> xxx,lz.yyy
     * xxx.arc.lz -> xxx,lz,arc (*)
     * Etc.
     * (*) The order is reversed, since packing is done in the reverse order.
     * Similarly, if a file is packed, its comma-extensions are changed to extensions.
     *
     * Comma-extensions make everything pretty simple, aren't really annoying
     * (its location doesn't alter file sorting by name nor real file extensions),
     * and are straightforward to understand (I hope!).
     */

    /// <summary>
    /// Class used to pack or unpack F-Zero GX files and directories.
    /// </summary>
    public class GxExpander
    {
        /// <summary>
        /// Get or set the game on which the GxExpander class operates.
        /// </summary>
        public GxGame Game { get; set; }

        /// <summary>
        /// Get or set the operation mode of the GxExpander class.
        /// </summary>
        public GxExpanderMode Mode { get; set; }

        /// <summary>
        /// Get or set the input path for packing/unpacking.
        /// </summary>
        public string InputPath { get; set; }

        /// <summary>
        /// Get or set the output path for packing/unpacking.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Allows to specify a BackgroundWorker in order to support cancellation.
        /// </summary>
        public BackgroundWorker BackgroundWorker { get; set; }

        /// <summary>
        /// Run the packing/unpacking process.
        /// </summary>
        public void Run()
        {
            // Verify that the class parameters are valid
            if (Game != GxGame.FZeroGX && Game != GxGame.SuperMonkeyBall && Game != GxGame.SuperMonkeyBallDX)
                throw new GxExpanderException("Please specify a valid game.");
            if (Mode != GxExpanderMode.Unpack && Mode != GxExpanderMode.Pack)
                throw new GxExpanderException("Please specify a valid operation mode.");
            if (string.IsNullOrEmpty(InputPath))
                throw new GxExpanderException("Please specify an input file/folder.");
            if (string.IsNullOrEmpty(OutputPath))
                throw new GxExpanderException("Please specify an output file/folder.");

            // Verify that the input exists
            FileSystemInfo input;
            if (File.Exists(InputPath))
            {
                input = new FileInfo(InputPath);
            }
            else if (Directory.Exists(InputPath))
            {
                input = new DirectoryInfo(InputPath);
            }
            else
                throw new GxExpanderException("The input does not exist.");

            // Verify that the output doesn't exist
            // The only case we allow, is if the output directory exists, but is empty
            if (File.Exists(OutputPath))
                throw new GxExpanderException("The output already exists as a file.");
            else if (Directory.Exists(OutputPath) && Directory.GetFileSystemEntries(OutputPath).Length != 0)
                throw new GxExpanderException("The output already exists as a directory.");

            RunRecursive(input, InputPath, OutputPath, true);
        }

        /// <summary>
        /// Runs the expander recursively on a file or directory.
        /// </summary>
        /// <param name="currentInput">FileSystemInfo object associated to the current input file/directory.</param>
        /// <param name="currentInputPath">Path to the current input file/directory.</param>
        /// <param name="currentOutputPath">Path the the current output file/directory.</param>
        /// <param name="keepOutputPath">
        /// true to avoid modifying the output path to introduce comma/dot-extensions.
        /// This is set at the top level in order to always respect the file name given by the user, and set to false elsewhere.
        /// </param>
        private void RunRecursive(FileSystemInfo currentInput, string currentInputPath, string currentOutputPath, bool keepOutputPath)
        {
            if (currentInput is DirectoryInfo)
            {
                DirectoryInfo currentInputDirectory = (DirectoryInfo)currentInput;
                RunForDirectory(currentInputDirectory, currentInputPath, currentOutputPath, keepOutputPath);
            }
            else if (currentInput is FileInfo)
            {
                using (FileStream currentInputFile = File.OpenRead(currentInput.FullName))
                {
                    RunForFile(currentInputFile, currentInputPath, currentOutputPath, keepOutputPath);
                }
            }
        }

        private void RunForDirectory(DirectoryInfo currentInputDirectory, string currentInputPath, string currentOutputPath, bool keepOutputPath)
        {
            if (Mode == GxExpanderMode.Pack && GetCommaExtension(currentInputPath) == "arc")
            {
                // Pack directory to .ARC file
                using (MemoryStream outputStream = new MemoryStream())
                {
                    ArcContainer arc = new ArcContainer(currentInputDirectory.FullName);
                    arc.Save(outputStream);
                    outputStream.Position = 0;

                    // Need to continue packing for ,lz,arc
                    RunForFile(outputStream, CommaExtToDotExt(currentInputPath),
                        !keepOutputPath ? CommaExtToDotExt(currentOutputPath) : currentOutputPath, keepOutputPath);
                }
            }
            else
            {
                // Continue recursively packing/unpacking the directory

                // Recreate the directory on the output
                Directory.CreateDirectory(currentOutputPath);

                // Iterate recursively through all files in the directory
                foreach (FileSystemInfo currentSubInput in currentInputDirectory.GetFileSystemInfos())
                {
                    RunRecursive(currentSubInput, Path.Combine(currentInputPath, currentSubInput.Name),
                        Path.Combine(currentOutputPath, currentSubInput.Name), false);

                    // Allow the user to cancel the process
                    if (BackgroundWorker != null && BackgroundWorker.CancellationPending)
                    {
                        return;
                    }
                }
            }
        }

        private void RunForFile(Stream currentInputStream, string currentInputPath, string currentOutputPath, bool keepOutputPath)
        {
            if (Mode == GxExpanderMode.Unpack && GetDotExtension(currentInputPath) == "lz")
            {
                // Unpack .LZ file
                using (Stream outputStream = new MemoryStream())
                {
                    Lz.Unpack(currentInputStream, outputStream, Game);
                    outputStream.Position = 0;

                    // Need to continue unpacking for .arc.lz
                    RunForFile(outputStream, DotExtToCommaExt(currentInputPath),
                        !keepOutputPath ? DotExtToCommaExt(currentOutputPath) : currentOutputPath, keepOutputPath);
                }
            }
            else if (Mode == GxExpanderMode.Pack && GetCommaExtension(currentInputPath) == "lz")
            {
                // Pack .LZ file
                using (Stream outputStream = new MemoryStream())
                {
                    Lz.Pack(currentInputStream, outputStream, Game);
                    outputStream.Position = 0;

                    RunForFile(outputStream, CommaExtToDotExt(currentInputPath),
                        !keepOutputPath ? CommaExtToDotExt(currentOutputPath) : currentOutputPath, keepOutputPath);
                }
            }
            else if (Mode == GxExpanderMode.Unpack && GetDotExtension(currentInputPath) == "arc")
            {
                // Unpack .ARC container

                ArcContainer arc = new ArcContainer(currentInputStream);
                arc.Extract(!keepOutputPath ? DotExtToCommaExt(currentOutputPath) : currentOutputPath);

                // No need to continue unpacking inside .arc files
            }
            else
            {
                // Copy the file as-is without change
                using (FileStream currentOutputStream = File.Create(currentOutputPath))
                {
                    currentInputStream.CopyTo(currentOutputStream);
                }
            }
        }

        /// <summary>
        /// Gets the last comma-extension for the given type.
        /// </summary>
        /// <param name="path">The path from which to get the extension.</param>
        /// <returns>The first comma-extension of the given type.</returns>
        private static string GetCommaExtension(string path)
        {
            string fileName = Path.GetFileName(path);

            int lastCommaExtPosStart = fileName.LastIndexOf(',');
            if (lastCommaExtPosStart == -1)
                return null;

            int lastCommaExtPosEnd = fileName.IndexOf('.', lastCommaExtPosStart);
            if (lastCommaExtPosEnd == -1)
                lastCommaExtPosEnd = fileName.Length;

            return fileName.Substring(lastCommaExtPosStart+1, lastCommaExtPosEnd - lastCommaExtPosStart-1);
        }

        /// <summary>
        /// Gets the last dot-extension for the given type.
        /// </summary>
        /// <param name="path">The path from which to get the extension.</param>
        /// <returns>The first dot-extension of the given type.</returns>
        private static string GetDotExtension(string path)
        {
            string fileName = Path.GetFileName(path);

            int lastDotExtPosStart = fileName.LastIndexOf('.');
            if (lastDotExtPosStart == -1)
                return null;

            int lastDotExtPosEnd = fileName.IndexOf(',', lastDotExtPosStart);
            if (lastDotExtPosEnd == -1)
                lastDotExtPosEnd = fileName.Length;

            return fileName.Substring(lastDotExtPosStart+1, lastDotExtPosEnd - lastDotExtPosStart-1);
        }

        /// <summary>
        /// Moves the rightmost "dot extension" to the leftmost "comma extension".
        /// E.g. from '.' to ',': file.arc.lz -> file,lz.arc
        /// E.g. from '.' to ',': file,lz.arc -> file,lz,arc
        /// E.g. from '.' to ',': file.gma.lz -> file,lz.gma
        /// </summary>
        /// <param name="path">The path to modify.</param>
        /// <returns>The modifed path.</returns>
        private static string DotExtToCommaExt(string path)
        {
            string fileName = Path.GetFileName(path);

            // Get range of last dot extension
            int lastDotExtPosStart = fileName.LastIndexOf('.');
            if (lastDotExtPosStart == -1)
                return null;

            int lastDotExtPosEnd = fileName.IndexOf(',', lastDotExtPosStart);
            if (lastDotExtPosEnd == -1)
                lastDotExtPosEnd = fileName.Length;

            string lastDotExt = fileName.Substring(lastDotExtPosStart + 1, lastDotExtPosEnd - lastDotExtPosStart - 1);

            // Remove last dot extension from name
            string fileNameWithoutExt = fileName.Substring(0, lastDotExtPosStart) + fileName.Substring(lastDotExtPosEnd);

            // Add it before all other dot extensions, or otherwise, at the end
            int firstDotExtPosStart = fileNameWithoutExt.IndexOf(".");
            if (firstDotExtPosStart == -1)
                firstDotExtPosStart = fileNameWithoutExt.Length;

            int firstDotExtPos = fileName.IndexOf('.');

            string newFileName = fileNameWithoutExt.Substring(0, firstDotExtPosStart) + "," + lastDotExt + fileNameWithoutExt.Substring(firstDotExtPosStart);

            return Path.Combine(Path.GetDirectoryName(path), newFileName);
        }

        /// <summary>
        /// Moves the rightmost "comma extension" to the leftmost "dot extension".
        /// E.g. from ',' to '.': file,lz,arc -> file.arc,lz
        /// E.g. from ',' to '.': file.arc,lz -> file.arc.lz
        /// E.g. from ',' to '.': file,lz.gma -> file.gma.lz
        /// </summary>
        /// <param name="path">The path to modify.</param>
        /// <returns>The modifed path.</returns>
        private static string CommaExtToDotExt(string path)
        {
            string fileName = Path.GetFileName(path);

            // Get range of last comma extension
            int lastCommaExtPosStart = fileName.LastIndexOf(',');
            if (lastCommaExtPosStart == -1)
                return null;

            int lastCommaExtPosEnd = fileName.IndexOf('.', lastCommaExtPosStart);
            if (lastCommaExtPosEnd == -1)
                lastCommaExtPosEnd = fileName.Length;

            string lastCommaExt = fileName.Substring(lastCommaExtPosStart+1, lastCommaExtPosEnd-lastCommaExtPosStart-1);

            // Remove last comma extension from name
            string fileNameWithoutExt = fileName.Substring(0, lastCommaExtPosStart) + fileName.Substring(lastCommaExtPosEnd);

            // Add it before all other comma extensions, or otherwise, at the end
            int firstCommaExtPosStart = fileNameWithoutExt.IndexOf(",");
            if (firstCommaExtPosStart == -1)
                firstCommaExtPosStart = fileNameWithoutExt.Length;

            string newFileName = fileNameWithoutExt.Substring(0, firstCommaExtPosStart) + "." + lastCommaExt + fileNameWithoutExt.Substring(firstCommaExtPosStart);

            return Path.Combine(Path.GetDirectoryName(path), newFileName);
        }
    }
}
