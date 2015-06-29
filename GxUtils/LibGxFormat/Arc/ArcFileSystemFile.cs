using System;
using System.IO;

namespace LibGxFormat.Arc
{
    /// <summary>
    /// A file in an ARC container.
    /// </summary>
    public class ArcFileSystemFile : ArcFileSystemEntry
    {
        /// <summary>
        /// Backing field for the contents of the file.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Contents of the file.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return data;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                data = value;
            }
        }

        /// <summary>
        /// Create a new ArcFileSystemFile with the given attributes.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="data">The contents of the file.</param>
        public ArcFileSystemFile(string name, byte[] data)
            : base(name)
        {
            this.Data = data;
        }

        /// <summary>
        /// Extract the contents of this ARC file to a directory.
        /// </summary>
        /// <param name="outputPath">The path of the output directory.</param>
	    public override void Extract(string outputPath)
	    {
            File.WriteAllBytes(Path.Combine(outputPath, Name), Data);
	    }
    }
}
