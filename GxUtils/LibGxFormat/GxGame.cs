using System.ComponentModel;

namespace LibGxFormat
{
	/// <summary>
	/// Defines the different games supported by this library.
	/// Some functions in this library require the game specification
	/// in order to handle game-specific differences.
	/// </summary>
	public enum GxGame
	{
        /// <summary>Super Monkey Ball 1 or 2</summary>
        [Description("SMB 1/2")]
		SuperMonkeyBall,
        /// <summary>Super Monkey Ball Deluxe </summary>
        [Description("SMB Deluxe")]
        SuperMonkeyBallDX,
        /// <summary>F-Zero GX</summary>
        [Description("F-Zero GX")]
        FZeroGX
    }
}

