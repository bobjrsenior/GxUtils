using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibGxFormat.Gma
{
    public class GcmfTriangleStripGroup
    {
        List<GcmfTriangleStrip> strips = new List<GcmfTriangleStrip>();

        internal void Render(IRenderer renderer, GcmfRenderContext context)
        {
            foreach (GcmfTriangleStrip strip in strips)
                strip.Render(renderer, context);
        }

        internal bool IsEmpty
        {
            get
            {
                return strips.Count == 0;
            }
        }

        internal void LoadNonIndexed(EndianBinaryReader input, int size, uint vertexFlags)
        {
            // AFAIK the number of strips isn't written anywhere, it should be found like below
            int endOffset = Convert.ToInt32(input.BaseStream.Position) + size;

            if (input.ReadByte() != 0)
                throw new InvalidGmaFileException("Expected GcmfObjectPart[0x00] == 0");

            while (Convert.ToInt32(input.BaseStream.Position) != endOffset)
            {
                GcmfTriangleStrip strip = new GcmfTriangleStrip();
                if (!strip.LoadNonIndexed(input, vertexFlags))
                    break;
                strips.Add(strip);
            }

            if (Convert.ToInt32(input.BaseStream.Position) + 0x20 < endOffset)
                throw new InvalidGmaFileException("GcmfObjectPart: Premature end of triangle strip group.");

            input.BaseStream.Position = endOffset;
        }

        internal int SizeOfNonIndexed()
        {
            return PaddingUtils.Align(1 + strips.Sum(strip => strip.SizeOfNonIndexed()), 0x20);
        }

        internal void SaveNonIndexed(EndianBinaryWriter output)
        {
            output.Write((byte)0);
            foreach (GcmfTriangleStrip strip in strips)
                strip.SaveNonIndexed(output);
            // Here, it's NOT mandatory to write a 0x00 to end the triangle strip list
            // As can be seen in the read loop, the end condition is either find a 0x00
            // OR end of size. In the case where we are aligned at a 0x20 boundary,
            // a zero should NOT be written in order to replicate the original game files
            output.Align(0x20);
        }

        internal void LoadIndexed(EndianBinaryReader input, int size, IList<GcmfVertex> vertexPool, uint vertexFlags)
        {
            for (int nIntsRead = 0; nIntsRead < size; )
            {
                GcmfTriangleStrip strip = new GcmfTriangleStrip();
                nIntsRead += strip.LoadIndexed(input, vertexPool, vertexFlags);
                strips.Add(strip);
            }
        }

        internal int SizeOfIndexed()
        {
            return strips.Sum(strip => strip.SizeOfIndexed());
        }

        internal void SaveIndexed(EndianBinaryWriter output)
        {
            foreach (GcmfTriangleStrip strip in strips)
                strip.SaveIndexed(output);
        }
    }
}
