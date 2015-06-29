using MiscUtil.Conversion;
using MiscUtil.IO;
using System.Collections.Generic;
using System.IO;

namespace LibGxFormat.Arc
{
    public class ArcContainer
    {
        private const uint ArcMagic = 0x55AA382D;

        private class U8Node
        {
            public const byte TYPE_FILE = 0x00;
            public const byte TYPE_DIRECTORY = 0x01;

            public byte Type; // 0=File, 1=Directory
            public int NameOffset; // (24 bits) Offset of the name in the string table.
            public int DataOffset; // For directories, the index of the parent directory.
            public int Size; // For directories, the index of the end node.

            public static U8Node Read(EndianBinaryReader s)
            {
                U8Node node = new U8Node();
                uint tmp = s.ReadUInt32();
                node.Type = (byte)(tmp >> 24);
                node.NameOffset = (int)(tmp & 0x00FFFFFF);
                node.DataOffset = s.ReadInt32();
                node.Size = s.ReadInt32();
                return node;
            }

            public void Write(EndianBinaryWriter s)
            {
                s.Write((uint)((Type << 24) | NameOffset));
                s.Write(DataOffset);
                s.Write(Size);
            }
        }

        /// <summary>
        /// Root node of the directory hierachy.
        /// </summary>
        public ArcFileSystemDirectory Root
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a new empty ArcContainer.
        /// </summary>
        public ArcContainer()
        {
            Root = new ArcFileSystemDirectory("");
        }

        /// <summary>
        /// Create a new ArcContainer from the contents of the given directory.
        /// </summary>
        /// <param name="inputDir">The path of the directory from which the files should be added.</param>
        public ArcContainer(string inputDir)
        {
            Root = new ArcFileSystemDirectory("", inputDir);
        }

        /// <summary>
        /// Create a new ArcContainer from the given .arc file.
        /// </summary>
        /// <param name="inputStream">Stream containing the .arc file.</param>
        public ArcContainer(Stream inputStream)
        {
            EndianBinaryReader inputBinaryStream = new EndianBinaryReader(EndianBitConverter.Big, inputStream);

            if (inputBinaryStream.ReadUInt32() != ArcMagic)
                throw new InvalidArcFileException("Invalid magic number.");
            int rootNodeOffset = inputBinaryStream.ReadInt32();
            int nodesAndStringTableSize = inputBinaryStream.ReadInt32();
            int dataOffset = inputBinaryStream.ReadInt32();

		    // Hackily get all the nodes and the offset of the string table
		    inputStream.Position = rootNodeOffset;

            U8Node rootNode = U8Node.Read(inputBinaryStream);
            U8Node[] nodes = new U8Node[rootNode.Size];
            nodes[0] = rootNode;
            for (int i = 1; i < rootNode.Size; i++)
                nodes[i] = U8Node.Read(inputBinaryStream);

            long stringTableBase = inputStream.Position;

            int nodeIdx = 0;
            Root = (ArcFileSystemDirectory)ParseNode(inputBinaryStream, stringTableBase, nodes, ref nodeIdx);
        }

        /// <summary>
        /// Generates a ArcFileSystemEntry from a U8Node.
        /// </summary>
        /// <param name="inputBinaryStream">The input stream containing the ARC container.</param>
        /// <param name="stringTableBase">Offset in the ARC of the string table.</param>
        /// <param name="nodes">Array of all U8Nodes in the ARC.</param>
        /// <param name="nodeIdx">
        /// The index of the node to parse.
        /// This will be updated to the index of the next node in the directory.
        /// </param>
        /// <returns>The ArcFileSystemEntry corresponding to the U8Node.</returns>
        private ArcFileSystemEntry ParseNode(EndianBinaryReader inputBinaryStream, long stringTableBase,
            U8Node[] nodes, ref int nodeIdx)
        {
            U8Node currentNode = nodes[nodeIdx++];

            inputBinaryStream.BaseStream.Position = stringTableBase + currentNode.NameOffset;
            string name = inputBinaryStream.ReadAsciiString();

		    if (currentNode.Type == U8Node.TYPE_FILE)
		    {
                inputBinaryStream.BaseStream.Position = currentNode.DataOffset;
                byte[] data = inputBinaryStream.ReadBytesOrThrow(currentNode.Size);
			    return new ArcFileSystemFile(name, data);
		    }
		    else if (currentNode.Type == U8Node.TYPE_DIRECTORY)
		    {
			    ArcFileSystemDirectory dir = new ArcFileSystemDirectory(name);

			    /* n.Size contains the index of the end entry in the directory.
			     * Since parseNode() updates nodeIdx to the next entry in the directory,
			     * we should entries until we reach the end of the directory. */
			    while (nodeIdx != currentNode.Size)
				    dir.Entries.Add(ParseNode(inputBinaryStream, stringTableBase, nodes, ref nodeIdx));

			    return dir;
		    }
		    else
            {
			    throw new InvalidArcFileException("Invalid node Type in ARC container.");
            }
        }

        /// <summary>
        /// Save the contents of this ARC container to an output stream.
        /// </summary>
        /// <param name="outputStream">The stream to which the ARC container will be written.</param>
        public void Save(Stream outputStream)
        {
            EndianBinaryWriter outputBinaryStream = new EndianBinaryWriter(EndianBitConverter.Big, outputStream);

            // Generate node structures. Assign identifiers, string table offsets, data offsets
            // Note that here the data offsets will be assigned relative to the data zone and not as absolute offsets
            // This will be fixed later, when we're able to calculate the file layout
            List<U8Node> nodes = new List<U8Node>();
            List<ArcFileSystemEntry> entries = new List<ArcFileSystemEntry>();
            int currentNodeId = 0;
            int currentStringTableOffset = 0;
            int currentDataOffset = 0;
            GenerateNodes(Root, nodes, entries, ref currentNodeId, 0, ref currentStringTableOffset, ref currentDataOffset);

            // Compute offsets of different parts of the file
            int headerSize = 0x20;
            int nodesSize = nodes.Count * 12;
            int stringTableSize = currentStringTableOffset;

            int rootNodeOffset = 0x20;
            int stringTableOffset = rootNodeOffset + nodesSize;
            int nodesAndStringTableSize = nodesSize + stringTableSize;
            int dataBeginOffset = PaddingUtils.Align(headerSize + nodesSize + stringTableSize, 0x20);

            foreach (U8Node node in nodes) // Convert data offsets from relative to absolute
            {
                if (node.Type == U8Node.TYPE_FILE)
                {
                    node.DataOffset += dataBeginOffset;
                }
            }

            // Write header
            outputBinaryStream.Write(ArcMagic);
            outputBinaryStream.Write(rootNodeOffset);
            outputBinaryStream.Write(nodesAndStringTableSize);
            outputBinaryStream.Write(dataBeginOffset);
            for (int i = 0; i < 0x10; i++)
                outputBinaryStream.Write((byte)0xCC);

            // Write nodes
            foreach (U8Node node in nodes)
                node.Write(outputBinaryStream);

            // Write string table
            foreach (ArcFileSystemEntry entry in entries)
                outputBinaryStream.WriteAsciiString(entry.Name);

            // Write file data
            foreach (ArcFileSystemEntry entry in entries)
            {
                if (entry is ArcFileSystemFile)
                {
                    outputBinaryStream.Align(0x20);
                    outputBinaryStream.Write(((ArcFileSystemFile)entry).Data);
                }
            }
        }

        private void GenerateNodes(ArcFileSystemEntry currentEntry, List<U8Node> nodes, List<ArcFileSystemEntry> entries,
            ref int currentNodeId, int parentNodeId, ref int currentStringTableOffset, ref int currentDataOffset)
        {
            // Add the node and the entry to the enumeration
            U8Node node = new U8Node();
            nodes.Add(node);
            entries.Add(currentEntry);

            int thisNodeId = currentNodeId;
            currentNodeId++;

            // Reserve space for the name in the string table
            node.NameOffset = currentStringTableOffset;
            currentStringTableOffset += currentEntry.Name.Length + 1;

            if (currentEntry is ArcFileSystemDirectory)
            {
                ArcFileSystemDirectory currentDirectory = (ArcFileSystemDirectory)currentEntry;

                foreach (ArcFileSystemEntry subEntry in currentDirectory.Entries)
                    GenerateNodes(subEntry, nodes, entries, ref currentNodeId, thisNodeId, ref currentStringTableOffset, ref currentDataOffset);

                node.Type = U8Node.TYPE_DIRECTORY;
                node.DataOffset = parentNodeId;
                node.Size = currentNodeId;
            }
            else if (currentEntry is ArcFileSystemFile)
            {
                ArcFileSystemFile currentFile = (ArcFileSystemFile)currentEntry;

                node.Type = U8Node.TYPE_FILE;
                node.DataOffset = currentDataOffset;                
                node.Size = currentFile.Data.Length;

                currentDataOffset = PaddingUtils.Align(currentDataOffset + currentFile.Data.Length, 0x20);
            }
        }

        /// <summary>
        /// Extract the contents of this ARC container to a directory.
        /// </summary>
        /// <param name="outputPath">The path of the output directory.</param>
        public void Extract(string outputPath)
        {
            Root.Extract(outputPath);
        }
    }
}
