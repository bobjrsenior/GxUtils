using OpenTK;
using MiscUtil.IO;

namespace LibGxFormat.Gma
{
    public class GcmfTransformMatrix
    {
        /// <summary>Backing storage for the matrix property.</summary>
        private Matrix3x4 matrixBackingStorage;

        /// <summary>4x4 matrix used to easily transform a vertex position by the matrix using OpenTK.</summary>
        private Matrix4 positionTransformMatrix;

        /// <summary>4x4 matrix used to easily transform a vertex normal by the matrix using OpenTK.</summary>
        private Matrix4 normalTransformMatrix;

        public Matrix3x4 Matrix
        {
            get
            {
                return matrixBackingStorage;
            }
            set
            {
                matrixBackingStorage = value;

                CalculatePositionAndNormalTransformMatrices();
            }
        }
        
        internal void Load(EndianBinaryReader input)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    matrixBackingStorage[y, x] = input.ReadSingle();
                }
            }

            CalculatePositionAndNormalTransformMatrices();
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
                    output.Write(matrixBackingStorage[y, x]);
                }
            }
        }

        private void CalculatePositionAndNormalTransformMatrices()
        {
            // We have to add the UnitW vector at the bottom in order to have a
            // proper transformation matrix that we can use with OpenTK
            positionTransformMatrix = new Matrix4(matrixBackingStorage.Row0,
                matrixBackingStorage.Row1, matrixBackingStorage.Row2, Vector4.UnitW);

            // OpenTK uses the Direct3D/XNA matrix multiplication order, in which vectors and matrices
            // are multiplied v*M instead of M*v. Transpose the matrix in order to reverse this issue.
            positionTransformMatrix.Transpose();

            // Calculate the inverse matrix for faster normal transforms.
            normalTransformMatrix = positionTransformMatrix.Inverted();
        }

        /// <summary>
        /// Transform the given position vector by the transformation matrix.
        /// This is equivalent to Matrix * (pos.X, pos.Y, pos.Z, 1).
        /// </summary>
        /// <param name="pos">The position vector to transform.</param>
        /// <returns>The transformed position vector.</returns>
        public Vector3 TransformPosition(Vector3 pos)
        {
            Vector3 result;
            Vector3.TransformPosition(ref pos, ref positionTransformMatrix, out result);
            return result;
        }

        /// <summary>
        /// Transform the given normal vector by the transformation matrix.
        /// This is equivalent to Inverse(Transpose(Matrix3x3)) * (nrm.X, nrm.Y, nrm.Z).
        /// </summary>
        /// <param name="nrm">The normal vector to transform.</param>
        /// <returns>The transformed normal vector.</returns>
        public Vector3 TransformNormal(Vector3 nrm)
        {
            Vector3 result;
            Vector3.TransformNormalInverse(ref nrm, ref normalTransformMatrix, out result);
            return result;
        }

    }
}
