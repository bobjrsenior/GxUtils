
namespace LibGxFormat.Arc
{
    /// <summary>
    /// Base class for files and directories in an ARC container.
    /// </summary>
    public abstract class ArcFileSystemEntry
    {
        /// <summary>
        /// The name of this entry.
        /// </summary>
        public string Name;

        /// <summary>
        /// Create a new ArcFileSystemEntry with the given name.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        public ArcFileSystemEntry(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Extract the contents of this ARC entry to a directory.
        /// </summary>
        /// <param name="outputPath">The path of the output directory.</param>
        public abstract void Extract(string outputPath);
    }
}
