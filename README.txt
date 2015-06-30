GxModelViewer - A F-Zero GX model viewer and editor
---------------------------------------------------
---------------------------------------------------

Known bugs
----------
----------

When exporting to OBJ/MTL, texture wrapping doesn't work
--------------------------------------------------------
That's because texture wrapping isn't supported on those formats. I'll attempt to figure out
some way to hack it into or switch to another model format.

On Super Monkey Ball, st137/st137.tpl fails the TPL repacking test
------------------------------------------------------------------
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

(Not a bug) On Super Monkey Ball, preview/*.tpl fails the TPL repacking test
----------------------------------------------------------------------------
This is because those files are actually headerless image files, instead of TPL files.
They aren't supported to pass the TPL repacking test.