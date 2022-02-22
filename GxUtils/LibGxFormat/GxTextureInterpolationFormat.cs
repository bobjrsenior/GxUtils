using System.ComponentModel;


namespace LibGxFormat
{

    /// <summary>
    /// Defines the different interpolation formats
    /// </summary>
    public enum GxInterpolationFormat
    {
        /// <summary>Uses the top left pixel when interpolating</summary>
        [Description("Nearest Neighbor")]
        NearestNeighbor,
        /// <summary>Uses C# default interpolation</summary>
        [Description("C# Default")]
        CSharpDefault,
        /// <summary>Uses the C# high-quality bilinear interploation algorithm</summary>
        [Description("High Quality Bicubic")]
        HighQualityBicubic,
    }
}
