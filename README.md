# GxModelViewer - A F-Zero GX model viewer and editor

## Introduction

This is a fork of GX Model Viewer that features several improvements over the original version. The complete list of changes is found within CHANGES.txt.

This specific fork of GX Model Viewer was developed for use with, and tested with the Super Monkey Ball series. No testing has been done with F-Zero GX.

In short, the following major features have been added:
	* Higher quality default mipmap interpolation algorithm (bicubic)
	* Ability to batch import and export of multiple texture mipmap levels
	* Ability to create new materials
	* Ability to import new textures, delete existing textures, or clean up all unused textures in a TPL
	* Ability to import multiple textures at a time
	* Ability to save and load headerless TPLs without the need of hex editing
	* Ability to use secondary and tertiary materials without the need of hex editing
	* Ability to delete, re-order, and translate models
	* Ability to scale primary and secondary texture coordinates, as well as the ability to create secondary coordinates from primary coordinates
	* Ability to import and export individual models as GMA/TPLs
	
These features, respectively, enable you to:
	* Use custom textures in a custom level without a fade-in effect appearing on textures after a certain distance.
	* Use pre-generated mipmaps or vanilla textures without having to tediously import each mipmap level
	* Add new materials to a model without having to use workarounds in your 3D modelling program
	* Add new textures without having to use workaround in your 3D modelling program, as well as the ability to decrease the filesize of a TPL
	* Modify the story mode/pratice mode/replay preview image TPL file in the Super Monkey Ball series
	* Quickly import large amounts of textures without having to tediously import them one-by-one
	* Add specular maps or other effects to models
	* Reduce the size of a GMA file, remove unwanted models, move models without having to resort to exporting the GMA to a 3D modelling program
	* Recreate effects similar to the red/green star overlay textures in the Bonus 2 world in Super Monkey Ball 2
	* Use custom bumper or goal models without needing to use GMATool
	
Most of these new features are available through the context menus, found by right-clicking on items in the model, material, or texture tabs. 

## Downloads

Windows binaries can be downloaded here:

https://github.com/TheBombSquad/GxUtils/releases

For Linux users, GX Model Viewer should work on the latest version of Wine without any special setup required. 
It can also be built and run using Mono, but all references to `GetConsoleWindow()` need to be commented out first.

## Headerless TPL modification

GX Model Viewer should detect if a TPL file is loaded without a proper header. You will then be prompted
to create a header, so that the TPL is properly loaded with the correct number of textures and mipmap levels.
The software will attempt to derive the correct parameters from the filename and file size of the loaded TPL.
It will be assumed that you are loading a texture with only one mipmap level, and the resolution of all of the
textures are uniform. The resolution is obtained from the file name - for example, the textures in the Super Monkey 
Ball 2 file replist160x112.tpl are all of the resolution 160x112. The number of textures is derived from the file size.

If a headerless TPL is loaded, the software will automatically save it without a header, and the button for saving a TPL
will change its label to "Save Headerless TPL". If you wish to save it with a header, you can hold Shift before pressing the
Save button, and the TPL will be saved with a header, using the parameters chosen upon the loading of the TPL. The label of the
Save button should change accordingly, when the Shift key is pressed.

If you wish to save a TPL with a header as one without one, you can hold Shift before pressing the Save button, and the TPL will
be saved without a header. The label of the Save button should change accordingly, when the Shift key is pressed.

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
            -importObjMtl <model>          Imports the designated .obj file.
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
