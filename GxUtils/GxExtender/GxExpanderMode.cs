namespace GxExpander
{
    /// <summary>
    /// Mode setting (unpack/pack) for the GxExpander class.
    /// </summary>
    public enum GxExpanderMode
    {
        /// <summary>
        /// Unpack: Uncompress LZ files, extract ARC files.
        /// </summary>
        Unpack,
        /// <summary>
        /// Pack: Compress LZ files, bundle ARC files.
        /// </summary>
        Pack
    }
}
