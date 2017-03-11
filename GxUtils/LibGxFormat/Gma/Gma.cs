using LibGxFormat.ModelLoader;
using LibGxFormat.ModelRenderer;
using MiscUtil.Conversion;
using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// A Gma model holds various entries containing Gcmf model objects.
    /// The container can contain null values (corresponding to empty entries in the .GMA stream header).
    /// </summary>
    public class Gma : Collection<GmaEntry>, IRenderable
    {
        /// <summary>
        /// Create a new Gma model containing no objects.
        /// </summary>
        public Gma()
        {
        }

        /// <summary>
        /// Create a new Gma model from a loaded OBJ/MTL model.
        /// </summary>
        /// <param name="model">The model to load the .GMA file from.</param>
        /// <param name="textureIndexMapping">Correspondence between the textures defined in the model materials and .TPL texture indices.</param>
        public Gma(ObjMtlModel model, Dictionary<Bitmap, int> textureIndexMapping)
            : this()
        {
            if (model.Objects.ContainsKey("") && model.Objects[""].Meshes.SelectMany(m => m.Faces).Any())
                throw new InvalidOperationException("Geometry is not allowed outside of named objects.");

            foreach (KeyValuePair<string, ObjMtlObject> objectEntry in model.Objects)
            {
                Gcmf modelObject = new Gcmf(objectEntry.Value, textureIndexMapping);
                this.Add(new GmaEntry(objectEntry.Key, modelObject));
            }
        }

        /// <summary>
        /// Create a new instance of a Gma model from a .GMA stream.
        /// </summary>
        /// <param name="inputStream">The stream that contains the .GMA stream.</param>
        /// <param name="game">The game from which the .TPL file is.</param>
        public Gma(Stream inputStream, GxGame game)
        {
			if (inputStream == null)
				throw new ArgumentNullException("inputStream");
			if (!Enum.IsDefined(typeof(GxGame), game))
				throw new ArgumentOutOfRangeException("game");
            
            if(game == GxGame.SuperMonkeyBallDX)
            {
                Load(new EndianBinaryReader(EndianBitConverter.Little, inputStream), game);
            }
            else
            {
                Load(new EndianBinaryReader(EndianBitConverter.Big, inputStream), game);
            }
        }

        struct GmaEntryOffsets
        {
            public int ModelOffset;
            public int NameOffset;
        }

        /// <summary>
        /// Load a Gma model from the given .GMA stream.
        /// </summary>
        private void Load(EndianBinaryReader input, GxGame game)
        {
            int numObjs = input.ReadInt32();
            int modelBasePosition = input.ReadInt32();

            List<GmaEntryOffsets> entryOffs = new List<GmaEntryOffsets>();
            for (int i = 0; i < numObjs; i++)
            {
                entryOffs.Add(new GmaEntryOffsets
                {
                    ModelOffset = input.ReadInt32(),
                    NameOffset = input.ReadInt32()
                });
            }

            int nameBasePosition = Convert.ToInt32(input.BaseStream.Position);

            foreach (GmaEntryOffsets entryOff in entryOffs)
            {
                // There are some "empty" entries, without any data or a name.
                // We insert null into the container in this case.
                if (entryOff.ModelOffset != -1 || entryOff.NameOffset != 0)
                {
                    input.BaseStream.Position = nameBasePosition + entryOff.NameOffset;
                    string name = input.ReadAsciiString();

                    input.BaseStream.Position = modelBasePosition + entryOff.ModelOffset;
                    Gcmf model = new Gcmf();
                    model.Load(input, game);

                    Add(new GmaEntry(name, model));
                }
                else
                {
                    Add(null);
                }
            }
        }

        public void Load(ObjMtlModel model, Dictionary<Bitmap, int> textureIndexMapping, string name)
        {
            if (model.Objects.ContainsKey("") && model.Objects[""].Meshes.SelectMany(m => m.Faces).Any())
                throw new InvalidOperationException("Geometry is not allowed outside of named objects.");

            if (model.Objects.Count != 1)
                throw new InvalidObjMtlFileException("Only replace 1 model with 1 model");

            int entryIndex = GetEntryIndex(name);

            foreach (KeyValuePair<string, ObjMtlObject> objectEntry in model.Objects)
            {
                Gcmf modelObject = new Gcmf(objectEntry.Value, textureIndexMapping);
                this[entryIndex] = new GmaEntry(name, modelObject);
            }
        }

        /// <summary>
        /// Get the size in bytes of this Gma model when written to a .GMA stream.
        /// </summary>
        /// <returns></returns>
        public int SizeOf()
        {
            return SizeOfHeader() + SizeOfModels();
        }

        private int SizeOfHeader()
        {
            return PaddingUtils.Align(SizeOfHeaderEntries() + SizeOfHeaderNameTable(), 0x20);
        }

        private int SizeOfHeaderEntries()
        {
            return 4 + 4 + (4 + 4) * Items.Count;
        }

        private int SizeOfHeaderNameTable()
        {
            return Items.Where(e => e != null).Sum(e => e.Name.Length + 1) + 1 /* File weirdness, see Write() */;
        }

        private int SizeOfModels()
        {
            return Items.Where(e => e != null).Sum(e => e.ModelObject.SizeOf());
        }

        /// <summary>
        /// Save this instance of a Gma model to a .GMA stream.
        /// </summary>
        /// <param name="outputStream">The input stream to which to write the .GMA stream.</param>
        /// <param name="game">The game from which the .GMA stream is.</param>
        public void Save(Stream outputStream, GxGame game)
        {
			if (outputStream == null)
				throw new ArgumentNullException("outputStream");
			if (!Enum.IsDefined(typeof(GxGame), game))
				throw new ArgumentOutOfRangeException("game");

            if (game == GxGame.SuperMonkeyBallDX)
            {
                Save(new EndianBinaryWriter(EndianBitConverter.Little, outputStream), game);
            }
            else
            {
                Save(new EndianBinaryWriter(EndianBitConverter.Big, outputStream), game);
            }

        }

        /// <summary>
        /// Save a Gma model to the given .GMA stream.
        /// </summary>
        private void Save(EndianBinaryWriter output, GxGame game)
        {
            int modelBasePosition = SizeOfHeader();

            // Write header entries
            output.Write(Items.Count);
            output.Write(modelBasePosition);

            int nameCurrentOffset = 0, modelCurrentOffset = 0;
            foreach (GmaEntry entry in Items)
            {
                if (entry != null)
                {
                    output.Write(modelCurrentOffset);
                    output.Write(nameCurrentOffset);

                    nameCurrentOffset += entry.Name.Length + 1;
                    modelCurrentOffset += entry.ModelObject.SizeOf();
                }
                else
                {
                    output.Write(-1);
                    output.Write(0);
                }
            }

            // Write name table
            foreach (GmaEntry entry in Items.Where(e => e != null))
                output.WriteAsciiString(entry.Name);

            // The alignment of the name table can be easily checked to be 0x20
            // However, there's a bug on the official files (such as init/sel.gma),
            // in which the files that fall exactly on the 0x20 alignment boundary
            // have an extra 0x20 bytes of padding.
            // To work around this weirdness and generate the same output as the originals,
            // we add an extra byte after the name table, which will make the
            // files that fall exactly on the 0x20 boundary to get the desired extra 0x20 bytes.
            output.Write((byte)0);
            output.Align(0x20);

            // Write models
            foreach (GmaEntry entry in Items.Where(e => e != null))
                entry.ModelObject.Save(output, game);
        }

        public List<int> GetTextureIds(string name)
        {
            List<int> textureIds = new List<int>();
            foreach(GmaEntry entry in Items)
            {
                if(entry != null && entry.Name.Equals(name))
                {
                    foreach (GcmfMaterial mat in entry.ModelObject.Materials)
                    {
                        if (!textureIds.Contains(mat.TextureIdx))
                        {
                            textureIds.Add(mat.TextureIdx);
                        }
                    }
                    break;
                }
            }
            return textureIds;
        }

        private int GetEntryIndex(string name)
        {
            for(int i = 0; i < Items.Count; ++i)
            {
                GmaEntry entry = Items[i];
                if (entry.Name.Equals(name))
                {
                    return i;
                }
            }

            throw new InvalidGmaFileException("Gma Entry not found");
        }

        /// <summary>
        /// Render all models in this Gma model using the given model renderer.
        /// </summary>
        /// <param name="renderer">The model renderer to use to render the objects.</param>
        public void Render(IRenderer renderer) 
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            foreach (GmaEntry entry in Items)
            {
                if (entry != null)
                {
                    // Define a new object for the model
                    renderer.BeginObject(entry.Name);
                    entry.ModelObject.Render(renderer);
                    renderer.EndObject();
                }
                else
                {
                    // Define an empty model in order to make the entries in the GMA
                    // correspond correctly to the entries in the model tree
                    renderer.BeginObject("EmptyObject");
                    renderer.EndObject();
                }
            }
        }

        /// <summary>
        /// Render all models in this Gma model using the given model renderer.
        /// </summary>
        /// <param name="renderer">The model renderer to use to render the objects.</param>
        public List<int> Render(IRenderer renderer, string modelName)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            List<int> textureIds = new List<int>();

            foreach (GmaEntry entry in Items)
            {
                if (entry != null)
                {
                    if (entry.Name.Equals(modelName))
                    {
                        // Define a new object for the model
                        renderer.BeginObject(entry.Name);
                        entry.ModelObject.Render(renderer);
                        renderer.EndObject();
                        foreach(GcmfMaterial mat in entry.ModelObject.Materials)
                        {
                            if (!textureIds.Contains(mat.TextureIdx))
                            {
                                textureIds.Add(mat.TextureIdx);
                            }
                        }
                    }
                }
            }
            return textureIds;
        }
    };
}
