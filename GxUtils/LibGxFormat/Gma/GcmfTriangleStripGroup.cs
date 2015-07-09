using LibGxFormat.ModelLoader;
using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibGxFormat.Gma
{
    public class GcmfTriangleStripGroup : NonNullableCollection<GcmfTriangleStrip>
    {
        public GcmfTriangleStripGroup()
        {
        }

        public GcmfTriangleStripGroup(ObjMtlMesh objMesh)
            : this()
        {
            foreach (ObjMtlFace face in objMesh.Faces)
            {
                Items.Add(new GcmfTriangleStrip(this, face));
            }
        }

        internal void Render(IRenderer renderer, GcmfRenderContext context)
        {
            foreach (GcmfTriangleStrip strip in Items)
                strip.Render(renderer, context);
        }
        
        internal void LoadNonIndexed(EndianBinaryReader input, int size, uint vertexFlags, bool is16Bit)
        {
            // AFAIK the number of strips isn't written anywhere, it should be found like below
            int endOffset = Convert.ToInt32(input.BaseStream.Position) + size;

            if (input.ReadByte() != 0)
                throw new InvalidGmaFileException("Expected GcmfObjectPart[0x00] == 0");

            while (Convert.ToInt32(input.BaseStream.Position) != endOffset)
            {
                GcmfTriangleStrip strip = new GcmfTriangleStrip();
                if (!strip.LoadNonIndexed(input, vertexFlags, is16Bit))
                    break;
                Items.Add(strip);
            }

            if (Convert.ToInt32(input.BaseStream.Position) + 0x20 < endOffset)
                throw new InvalidGmaFileException("GcmfObjectPart: Premature end of triangle strip group.");

            input.BaseStream.Position = endOffset;
        }

        internal int SizeOfNonIndexed(bool is16Bit)
        {
            return PaddingUtils.Align(1 + Items.Sum(strip => strip.SizeOfNonIndexed(is16Bit)), 0x20);
        }

        internal void SaveNonIndexed(EndianBinaryWriter output, bool is16Bit)
        {
            output.Write((byte)0);
            foreach (GcmfTriangleStrip strip in Items)
                strip.SaveNonIndexed(output, is16Bit);
            // Here, it's NOT mandatory to write a 0x00 to end the triangle strip list
            // As can be seen in the read loop, the end condition is either find a 0x00
            // OR end of size. In the case where we are aligned at a 0x20 boundary,
            // a zero should NOT be written in order to replicate the original game files
            output.Align(0x20);
        }

        internal void LoadIndexed(EndianBinaryReader input, int size, OrderedSet<GcmfVertex> vertexPool, uint vertexFlags)
        {
            for (int nIntsRead = 0; nIntsRead < size; )
            {
                GcmfTriangleStrip strip = new GcmfTriangleStrip();
                nIntsRead += strip.LoadIndexed(input, vertexPool, vertexFlags);
                Items.Add(strip);
            }
        }

        internal int SizeOfIndexed()
        {
            return Items.Sum(strip => strip.SizeOfIndexed());
        }

        internal void SaveIndexed(EndianBinaryWriter output, Dictionary<GcmfVertex, int> vertexPool)
        {
            foreach (GcmfTriangleStrip strip in Items)
                strip.SaveIndexed(output, vertexPool);
        }
    }
}
