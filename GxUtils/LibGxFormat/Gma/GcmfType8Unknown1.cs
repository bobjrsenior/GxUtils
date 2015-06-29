using MiscUtil.IO;

namespace LibGxFormat.Gma
{
    public class GcmfType8Unknown1
    {
        float unk0;
        float unk4;
        float unk8;
        float unkC;
        float unk10;
        float unk14;
        internal ushort unk18; // TODO
        float unk1C;

        internal void Load(EndianBinaryReader input)
        {
            unk0 = input.ReadSingle();
            unk4 = input.ReadSingle();
            unk8 = input.ReadSingle();
            unkC = input.ReadSingle();
            unk10 = input.ReadSingle();
            unk14 = input.ReadSingle();
            unk18 = input.ReadUInt16();
            if (input.ReadUInt16() != 0)
                throw new InvalidGmaFileException("Expected GcmfType8Unknown1[0x1A] == 0");
            unk1C = input.ReadSingle();
        }

        internal int SizeOf()
        {
            return 0x20;
        }

        internal void Save(EndianBinaryWriter output)
        {
            output.Write(unk0);
            output.Write(unk4);
            output.Write(unk8);
            output.Write(unkC);
            output.Write(unk10);
            output.Write(unk14);
            output.Write(unk18);
            output.Write((ushort)0);
            output.Write(unk1C);
        }
    }
}
