# GxModelViewer - A F-Zero GX model viewer and editor
## Multiple mipmap level import/export
This version of GX Model Viewer supports the export/import of multiple mipmap levels at a time. 

To import multiple levels of mipmaps, first, place all of the mipmap levels into the same folder. They
do not have to be isolated from other image files. Ensure that the last two characters of the filename
are " 0" or "_0". Select the texture in the Textures tab (Texture 1, Texture 2, etc), and press the Import
button. Make sure you do not have any specific mipmap level selected. Select the file, then select the file 
format. All of the files should be imported as the respective mipmap levels. If you have any issues, make
sure tha the files are properly named, and in the correct order.

If you do not wish to import mipmap levels, and would rather have GX Model Viewer generate them, import the
texture as you normally would. Just ensure that the filename does not end in " 0" or "_0", or only one mipmap level
will be imported.

To export multiple levels of mipmaps, simply select the texture in the Textures tab (Texture 1, Texture 2, etc),
and press the Export button. Choose a location for level 0 of the texture. All of the mipmap levels will be
exported to the specified folder.

## Command line support
GX Model Viewer supports command line usage. It isn't as feature complete as the UI, but it
has what most people need.

The command line can be used with just command line flags or in interactive mode. Interative
mode is activated with the -interactive flag and cause commands to be read from standard input.
It reads one line of input at a time and tries to split it into commands/arguments. Then operates
on them just like command line flags. This means multiple commands can be in one line of input.

The only command difference in flags between non-interactive andmode  interactive mode is that the -interactive
flag is not available in interactive mode and the -quit flag is not available in non-interactive mode.

### Usage:
    GX Model Viewer Command Line Help
    Description:
            If no arguments are given, GxModelViewer starts in its normal GUI mode.
            Otherwise the GUI will not open and it becomes command line only.
            See the '-interactive' switch for interactive mode.
    Usage: .\GxModelViewer [arg [value...]...]

    args:
            -help                       Display this help.
            -interHelp                  Display help specific for interactive mode.
            -interactive                Start GxModelViewer in interactive mode.
                                        While in interactive mode, GX takes newline separated commands from stdin.
                                        See '-interhelp' for interactive specific commands and differences.
            -game <type>                The game to use when importing/exporting.
                                            smb: Super Monkey Ball 1/2 (default)
                                            deluxe: Super Monkey Ball Deluxe (beta)
                                            fzero: F-Zero GX
            -mipmaps <num>              The number of mipmaps to make on import.
            -interpolate <type>         The type of interpolation to use with mipmap generation.
                                            default: The C# default type (default)
                                            nearest: Nearest neighbor
                                            nn: Nearest Neighbor alias
            -importObj <model>          Imports the designated .obj file.
            -importTpl <texture>        Imports the designated .tpl file.
            -importGma <model>          Imports the designated .gma file.
            -exportObjMtl <model>       Exports the loaded model as a .obj/.mtl file.
            -exportTpl <textures>       Exports the loaded textures as a .tpl file.
            -exportGma <model>          Exports the loaded model as a .gma file.
            -setAllMipmaps <num>        Sets the number of mipmaps for every loaded texture to <num>.
                                        Texture files should be loaded before calling this flag.

### Interactive Mode Specifics
    GX Model Viewer Interactive Mode Help
    Description:
            GX Model Viewer Interactive mode works by reading lines from standard input.
            Each line of input works just like command line arguments, so each line can have multiple commands.
            Input continues to be read until the -quit command is recieved.
            The -interactive command does NOT do anything in interactive mode.

    Special Interactive Mode Args:
            -quit                       Quit interactive mode and exit the program.

## Known bugs

### When exporting to OBJ/MTL, texture wrapping doesn't work
That's because texture wrapping isn't supported on those formats. I'll attempt to figure out
some way to hack it into or switch to another model format.

### On Super Monkey Ball, st137/st137.tpl fails the TPL repacking test
This appears to be a bug of the original packer used to create the TPL files of the game.

In this file, there's a 256x32 texture in I8 format, with 6 mipmap levels,
which only has 0x2AC0 bytes of space allocated for it in the TPL file.

However, according to the I8 format, which has a tile size of (8x4) with 8bpp, the size should be:
 
    256x32 =        0x2000
    128x16 =         0x800
    64x8   =         0x200
    32x4   =          0x80
    16x2   -> 16x4 =  0x40
    8x1    -> 8x4  =  0x20
    ----------------------
                    0x2AE0

When reading this file, GxGma will read 0x2AE0 bytes of input, effectively taking an extra 0x20
bytes from whatever happens to be the next texture. This will "corrupt" the last 8x1 mipmap.

Since this problem, unlike the F-Zero GX CMPR bug, only affects a single texture on a single file,
and it shouldn't have any negative effect in practice, it's unlikely to be ever fixed.

### (Not a bug) On Super Monkey Ball, preview/*.tpl fails the TPL repacking test
This is because those files are actually headerless image files, instead of TPL files.
They aren't supported to pass the TPL repacking test.
