using OpenTK;
using MiscUtil.IO;

namespace LibGxFormat.Gma
{
    public class GcmfTransformMatrix
    {
        public Matrix3x4 Matrix;
        
        internal void Load(EndianBinaryReader input)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Matrix[y, x] = input.ReadSingle();
                }
            }
        }

        internal int SizeOf()
        {
            return 0x30;
        }

        internal void Save(EndianBinaryWriter output)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    output.Write(Matrix[y, x]);
                }
            }
        }
    }
}
