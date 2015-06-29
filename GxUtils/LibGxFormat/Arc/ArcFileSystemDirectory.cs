using System.Collections.Generic;
using System.IO;

namespace LibGxFormat.Arc
{
    /// <summary>
    /// Represents a directory inside an ARC container.
    /// </summary>
    public class ArcFileSystemDirectory : ArcFileSystemEntry
    {
	    /// <summary>
	    /// The file system entries contained in this directory.
	    /// </summary>
	    public List<ArcFileSystemEntry> Entries
        {
            get;
            private set;
        }

        /// <summary>
        /// Create an empty ArcFileSystemDirectory with the specified name.
        /// </summary>
	    public ArcFileSystemDirectory(string name)
            : base(name)
        {
            Entries = new List<ArcFileSystemEntry>();
        }

        /// <summary>
        /// Create an empty ArcFileSystemDirectory with the specified name.
        /// </summary>
        public ArcFileSystemDirectory(string name, string inputDir)
            : this(name)
        {
            AddFiles(inputDir);
        }

	    /// <summary>
        /// Add the contents of the specified path to the directory.
	    /// </summary>
	    /// <param name="inputPath">The path from which to add the files.</param>
	    public void AddFiles(string inputPath)
	    {
            DirectoryInfo inputDir = new DirectoryInfo(inputPath);

		    foreach (FileSystemInfo info in inputDir.GetFileSystemInfos())
		    {
			    string fileName = Path.GetFileName(info.FullName);

			    if (info is DirectoryInfo)
				    Entries.Add(new ArcFileSystemDirectory(fileName, info.FullName));
			    else if (info is FileInfo)
                    Entries.Add(new ArcFileSystemFile(fileName, File.ReadAllBytes(info.FullName)));
		    }
	    }

        /// <summary>
        /// Extract the contents of this ARC directory to a directory.
        /// </summary>
        /// <param name="outputPath">The path of the output directory.</param>
        public override void Extract(string outputPath)
	    {
            string subPath = Path.Combine(outputPath, Name);
            Directory.CreateDirectory(subPath);

            foreach (ArcFileSystemEntry e in Entries)
                e.Extract(subPath);
	    }
    }
}
