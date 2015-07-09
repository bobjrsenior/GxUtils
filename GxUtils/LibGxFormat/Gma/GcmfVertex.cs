using LibGxFormat.ModelLoader;
using LibGxFormat.ModelRenderer;
using MiscUtil.IO;
using OpenTK;
using System;
using System.Drawing;

namespace LibGxFormat.Gma
{
    public class GcmfVertex
    {
        /// <summary>
        /// This value is used to initialize type 2 vertices, which are in a vertex pool.
        /// For this kind of vertices, we don't know the vertex type until we read the mesh data.
        /// So, when reading the vertex data, set this flag to true, and when we know the type, set it to false.
        /// </summary>
        bool indexedVertexPendingFlagsAssignment;

        public byte? TransformMatrixRef { get; set; }
        public Vector3 Position { get; set; }
        public Vector3? Normal { get; set; }
        public Color? VertexColor { get; set; }
        public Vector2? PrimaryTexCoord { get; set; }
        public Vector2? SecondaryTexCoord { get; set; }
        public Vector2? TertiaryTexCoord { get; set; }
        uint? unknown5_1, unknown5_2, unknown5_3, unknown5_4, unknown5_5, unknown5_6, unknown5_7, unknown5_8, unknown5_9;

        Color colorIndexedRaw;
        uint unknown7_1, unknown7_2, unknown7_3;

        /// <summary>
        /// Currently known vertex flags. Vertex fields are stored from the LSB to the MSB.
        /// </summary>
        [Flags]
        internal enum GcmfVertexFlags : uint
        {
            /// <summary>
            /// Reference to the transform matrix index table. uint8_t[1].
            /// This specifies a transformation matrix that will be done to the vertex.
            /// This value will always be a multiple of 3, up to 24.
            /// So the possible values are: 0,3,6,9,12,15,18,21,24 (9 different values).
            /// A value of 0 specifies that the vertex is NOT transformed by a transformation matrix.
            /// All other values specify that the vertex is transformed by the (value/3)-1 transformation matrix,
            /// as referenced in the mesh, or if not defined, in the model.
            /// (The mesh and the model contain a table of 8 indexes to the transform matrices to apply).
            /// </summary>
            TransformMatrixRef = 0x00000001,
            /// <summary>
            /// Vertex coordinates. float[3].
            /// This is ALWAYS set for all vertices in the official models,
            /// so we will just check that it's set and assume a vertex has coordinates everywhere.
            /// </summary>
            Coordinates = 0x00000200,
            /// <summary>
            /// Vertex normals. float[3]
            /// </summary>
            Normals = 0x00000400,
            /// <summary>
            /// Vertex color. uint32_t[1]
            /// </summary>
            Color = 0x00000800,
            /// <summary>
            /// Texture coordinates (U, V) for the primary material texture. float[2]
            /// </summary>
            PrimaryTextureCoordinates = 0x00002000,
            /// <summary>
            /// Texture coordinates (U, V) for the secondary material texture. float[2]
            /// </summary>
            SecondaryTextureCoordinates = 0x00004000,
            /// <summary>
            /// Texture coordinates (U, V) for the tertiary material texture. float[2]
            /// </summary>
            TertiaryTextureCoordinates = 0x00008000,
            /// <summary>
            /// Unknown. uint8_t[24]
            /// </summary>
            Unknown5 = 0x02000000
        }

        /// <summary>
        /// Calculate the vertex flags corresponding to this vertex.
        /// </summary>
        internal uint VertexFlags
        {
            get
            {
                return (uint)(
                    ((TransformMatrixRef != null) ? GcmfVertexFlags.TransformMatrixRef : 0) |
                    GcmfVertexFlags.Coordinates |
                    ((Normal != null) ? GcmfVertexFlags.Normals : 0) |
                    ((VertexColor != null) ? GcmfVertexFlags.Color : 0) |
                    ((PrimaryTexCoord != null) ? GcmfVertexFlags.PrimaryTextureCoordinates : 0) |
                    ((SecondaryTexCoord != null) ? GcmfVertexFlags.SecondaryTextureCoordinates : 0) |
                    ((TertiaryTexCoord != null) ? GcmfVertexFlags.TertiaryTextureCoordinates : 0) |
                    ((unknown5_1 != null) ? GcmfVertexFlags.Unknown5 : 0));
            }
        }

        /// <summary>
        /// Convert a 32-bit uint containing a RGBA color (red in highest byte, alpha in lowest byte) to a .NET System.Drawing.Color.
        /// </summary>
        /// <param name="colorRgba">The 32-bit uint containing a RGBA color.</param>
        /// <returns>The corresponding System.Drawing.Color.</returns>
        private static Color UnpackColorRGBA(uint colorRgba)
        {
            return Color.FromArgb(
                    (byte)(colorRgba >> 0), // Alpha
                    (byte)(colorRgba >> 24), // Red
                    (byte)(colorRgba >> 16), // Green
                    (byte)(colorRgba >> 8)); // Blue
        }

        /// <summary>
        /// Convert a .NET System.Drawing.Color to a 32-bit uint containing a RGBA color (red in highest byte, alpha in lowest byte).
        /// </summary>
        /// <param name="color">The System.Drawing.Color.</param>
        /// <returns>The corresponding 32-bit uint containing a RGBA color.</returns>
        private static uint PackColorRGBA(Color color)
        {
            return (uint)(
                    (color.A << 0) |
                    (color.R << 24) |
                    (color.G << 16) |
                    (color.B << 8));
        }

        public GcmfVertex()
        {
        }

        public GcmfVertex(ObjMtlVertex vtx)
        {
            this.Position = vtx.Position;
            this.Normal = vtx.Normal;
            this.PrimaryTexCoord = vtx.TexCoord;

            /* OBJ considers (0,0) to be the top left, GMA and OpenGL consider it to be the bottom left.
             * See http://stackoverflow.com/a/5605027 (Thanks to Tommy @ StackOverflow). */
            if (this.PrimaryTexCoord.HasValue)
                this.PrimaryTexCoord = new Vector2(this.PrimaryTexCoord.Value.X, 1 - this.PrimaryTexCoord.Value.Y);
        }

        private static float ReadNumberOfType(EndianBinaryReader input, bool is16Bit)
        {
            if (is16Bit)
            {
                return (float)input.ReadInt16() / 8191.0f;
            }
            else
            {
                return input.ReadSingle();
            }
        }

        internal void LoadNonIndexed(EndianBinaryReader input, uint vertexFlags, bool is16Bit)
        {
            // Make sure that only known flags are set
            if ((vertexFlags & (uint)~(GcmfVertexFlags.TransformMatrixRef |
                                       GcmfVertexFlags.Coordinates |
                                       GcmfVertexFlags.Normals |
                                       GcmfVertexFlags.Color |
                                       GcmfVertexFlags.PrimaryTextureCoordinates |
                                       GcmfVertexFlags.SecondaryTextureCoordinates |
                                       GcmfVertexFlags.TertiaryTextureCoordinates |
                                       GcmfVertexFlags.Unknown5)) != 0)
            {
                throw new InvalidGmaFileException("[GcmfVertexNonIndexed] Unknown vertex flags.");
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.Coordinates) == 0)
            {
                throw new InvalidGmaFileException("[GcmfVertexNonIndexed] No coordinates flag.");
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.TransformMatrixRef) != 0)
            {
                TransformMatrixRef = input.ReadByte();
            }

            Position = new Vector3(
                ReadNumberOfType(input, is16Bit),
                ReadNumberOfType(input, is16Bit),
                ReadNumberOfType(input, is16Bit));

            if ((vertexFlags & (uint)GcmfVertexFlags.Normals) != 0)
            {
                Normal = new Vector3(
                    ReadNumberOfType(input, is16Bit),
                    ReadNumberOfType(input, is16Bit),
                    ReadNumberOfType(input, is16Bit));
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.Color) != 0)
            {
                VertexColor = UnpackColorRGBA(input.ReadUInt32());
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.PrimaryTextureCoordinates) != 0)
            {
                PrimaryTexCoord = new Vector2(
                    ReadNumberOfType(input, is16Bit),
                    ReadNumberOfType(input, is16Bit));
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.SecondaryTextureCoordinates) != 0)
            {
                SecondaryTexCoord = new Vector2(
                    ReadNumberOfType(input, is16Bit),
                    ReadNumberOfType(input, is16Bit));
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.TertiaryTextureCoordinates) != 0)
            {
                TertiaryTexCoord = new Vector2(
                    ReadNumberOfType(input, is16Bit),
                    ReadNumberOfType(input, is16Bit));
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.Unknown5) != 0)
            {
                // TODO should use readNum?
                unknown5_1 = input.ReadUInt32();
                unknown5_2 = input.ReadUInt32();
                unknown5_3 = input.ReadUInt32();
                unknown5_4 = input.ReadUInt32();
                unknown5_5 = input.ReadUInt32();
                unknown5_6 = input.ReadUInt32();
                unknown5_7 = input.ReadUInt32();
                unknown5_8 = input.ReadUInt32();
                unknown5_9 = input.ReadUInt32();
            }
        }

        private static int SizeOfNumberOfType(bool is16Bit)
        {
            if (is16Bit)
            {
                return 2; // Save as uint16
            }
            else
            {
                return 4; // Save as float
            }
        }

        internal int SizeOfNonIndexed(bool is16Bit)
        {
            int size = 0;

            if (TransformMatrixRef != null)
                size += 1;

            size += SizeOfNumberOfType(is16Bit) * 3;

            if (Normal != null)
                size += SizeOfNumberOfType(is16Bit) * 3;

            if (VertexColor != null)
                size += 4;

            if (PrimaryTexCoord != null)
                size += SizeOfNumberOfType(is16Bit) * 2;

            if (SecondaryTexCoord != null)
                size += SizeOfNumberOfType(is16Bit) * 2;

            if (TertiaryTexCoord != null)
                size += SizeOfNumberOfType(is16Bit) * 2;

            if (unknown5_1 != null)
                size += 4 * 9;

            return size;
        }

        static void WriteNumberOfType(EndianBinaryWriter output, bool is16Bit, float value)
        {
            if (is16Bit)
            {
                output.Write((short)(Math.Round(value * 8191.0f)));
            }
            else
            {
                output.Write(value);
            }
        }

        internal void SaveNonIndexed(EndianBinaryWriter output, bool is16Bit)
        {
            if (TransformMatrixRef != null)
            {
                output.Write(TransformMatrixRef.Value);
            }

            WriteNumberOfType(output, is16Bit, Position.X);
            WriteNumberOfType(output, is16Bit, Position.Y);
            WriteNumberOfType(output, is16Bit, Position.Z);

            if (Normal != null)
            {
                WriteNumberOfType(output, is16Bit, Normal.Value.X);
                WriteNumberOfType(output, is16Bit, Normal.Value.Y);
                WriteNumberOfType(output, is16Bit, Normal.Value.Z);
            }

            if (VertexColor != null)
            {
                output.Write(PackColorRGBA(VertexColor.Value));
            }

            if (PrimaryTexCoord != null)
            {
                WriteNumberOfType(output, is16Bit, PrimaryTexCoord.Value.X);
                WriteNumberOfType(output, is16Bit, PrimaryTexCoord.Value.Y);
            }

            if (SecondaryTexCoord != null)
            {
                WriteNumberOfType(output, is16Bit, SecondaryTexCoord.Value.X);
                WriteNumberOfType(output, is16Bit, SecondaryTexCoord.Value.Y);
            }

            if (TertiaryTexCoord != null)
            {
                WriteNumberOfType(output, is16Bit, TertiaryTexCoord.Value.X);
                WriteNumberOfType(output, is16Bit, TertiaryTexCoord.Value.Y);
            }

            if (unknown5_1 != null)
            {
                output.Write(unknown5_1.Value);
                output.Write(unknown5_2.Value);
                output.Write(unknown5_3.Value);
                output.Write(unknown5_4.Value);
                output.Write(unknown5_5.Value);
                output.Write(unknown5_6.Value);
                output.Write(unknown5_7.Value);
                output.Write(unknown5_8.Value);
                output.Write(unknown5_9.Value);
            }
        }

        internal void LoadIndexed(EndianBinaryReader input)
        {
            indexedVertexPendingFlagsAssignment = true;

            Position = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
            // NOTE: this is not always an unit vector like on non-indexed ones!
            Normal = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
            PrimaryTexCoord = new Vector2(input.ReadSingle(), input.ReadSingle());
            SecondaryTexCoord = new Vector2(input.ReadSingle(), input.ReadSingle());
            TertiaryTexCoord = new Vector2(input.ReadSingle(), input.ReadSingle());
            
            // This field contains a color. It can be checked that, when set, this color is meaningful.
            // Save the value of this color even though the color flag may not be set later.
            colorIndexedRaw = UnpackColorRGBA(input.ReadUInt32());
            unknown7_1 = input.ReadUInt32(); // TODO
            unknown7_2 = input.ReadUInt32(); // TODO, only present with type=8
            unknown7_3 = input.ReadUInt32(); // TODO, only present with type=8
        }

        internal int SizeOfIndexed()
        {
            return 0x40;
        }

        internal void SaveIndexed(EndianBinaryWriter output)
        {
            output.Write(Position.X);
            output.Write(Position.Y);
            output.Write(Position.Z); 

            if (Normal != null)
            {
                output.Write(Normal.Value.X);
                output.Write(Normal.Value.Y);
                output.Write(Normal.Value.Z);
            }
            else
            {
                output.Write(0.0f);
                output.Write(0.0f);
                output.Write(0.0f);
            }

            if (PrimaryTexCoord != null)
            {
                output.Write(PrimaryTexCoord.Value.X);
                output.Write(PrimaryTexCoord.Value.Y);
            }
            else
            {
                output.Write(0.0f);
                output.Write(0.0f);
            }

            if (SecondaryTexCoord != null)
            {
                output.Write(SecondaryTexCoord.Value.X);
                output.Write(SecondaryTexCoord.Value.Y);
            }
            else
            {
                output.Write(0.0f);
                output.Write(0.0f);
            }

            if (TertiaryTexCoord != null)
            {
                output.Write(TertiaryTexCoord.Value.X);
                output.Write(TertiaryTexCoord.Value.Y);
            }
            else
            {
                output.Write(0.0f);
                output.Write(0.0f);
            }

            output.Write(PackColorRGBA(colorIndexedRaw));
            output.Write(unknown7_1);
            output.Write(unknown7_2);
            output.Write(unknown7_3);
        }

        internal void AssignVertexFlagsIndexed(uint vertexFlags)
        {
            // Make sure that only known flags are set
            if ((vertexFlags & (uint)~(GcmfVertexFlags.Coordinates |
                                       GcmfVertexFlags.Normals |
                                       GcmfVertexFlags.PrimaryTextureCoordinates |
                                       GcmfVertexFlags.SecondaryTextureCoordinates |
                                       GcmfVertexFlags.TertiaryTextureCoordinates |
                                       GcmfVertexFlags.Color)) != 0)
            {
                throw new InvalidGmaFileException("[GcmfVertexIndexed] Unknown vertex flags.");
            }

            if ((vertexFlags & (uint)GcmfVertexFlags.Coordinates) == 0)
            {
                throw new InvalidGmaFileException("[GcmfVertexIndexed] No coordinates flag.");
            }

            if (indexedVertexPendingFlagsAssignment)
            {
                // If this is the first time we assign the flags, then check that the readed values
                // from the fields with flags not set are zero, and set them to null to signify they aren't set

                if ((vertexFlags & (uint)GcmfVertexFlags.Normals) == 0)
                {
                    if (Normal.Value.X != 0.0f || Normal.Value.Y != 0.0f || Normal.Value.Z != 0.0f)
                    {
                        throw new InvalidGmaFileException("[GcmfVertexIndexed] Normal flags unset but nonzero normals.");
                    }

                    Normal = null;
                }

                if ((vertexFlags & (uint)GcmfVertexFlags.PrimaryTextureCoordinates) == 0)
                {
                    if (PrimaryTexCoord.Value.X != 0 || PrimaryTexCoord.Value.Y != 0)
                    {
                        throw new InvalidGmaFileException("[GcmfVertexIndexed] Primary texcoord flags unset but nonzero normals.");
                    }

                    PrimaryTexCoord = null;
                }

                if ((vertexFlags & (uint)GcmfVertexFlags.SecondaryTextureCoordinates) == 0)
                {
                    if (SecondaryTexCoord.Value.X != 0 || SecondaryTexCoord.Value.Y != 0)
                    {
                        throw new InvalidGmaFileException("[GcmfVertexIndexed] Secondary texcoord flags unset but nonzero normals.");
                    }

                    SecondaryTexCoord = null;
                }

                if ((vertexFlags & (uint)GcmfVertexFlags.TertiaryTextureCoordinates) == 0)
                {
                    if (TertiaryTexCoord.Value.X != 0 || TertiaryTexCoord.Value.Y != 0)
                    {
                        throw new InvalidGmaFileException("[GcmfVertexIndexed] Tertiary texcoord flags unset but nonzero normals.");
                    }

                    TertiaryTexCoord = null;
                }

                if ((vertexFlags & (uint)GcmfVertexFlags.Color) != 0)
                {
                    VertexColor = colorIndexedRaw;
                }

                indexedVertexPendingFlagsAssignment = false;
            }
            else
            {
                // If we got here, it means that this vertex in the vertex pool has been referenced from
                // two different places (nothing wrong with this). But, we want to make that the same vertex
                // flags were used in the same places
                if (vertexFlags != VertexFlags)
                {
                    throw new InvalidGmaFileException("[GcmfVertexIndexed] Trying assign two different flag sets.");
                }
            }
        }

        internal bool IsIndexedVertexInitialized()
        {
            return !indexedVertexPendingFlagsAssignment;
        }
    }
}
