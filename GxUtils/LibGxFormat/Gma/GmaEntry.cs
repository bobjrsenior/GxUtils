using System;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// An object entry in a Gma model.
    /// </summary>
    public class GmaEntry
    {
        string nameBackingStorage;

        /// <summary>Name of the object.</summary>
        public string Name
        {
            get
            {
                return nameBackingStorage;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                nameBackingStorage = value;
            }
        }

        Gcmf modelObjectBackingStorage;

        /// <summary>Gcmf model associated with this Gma object entry, or null if this is a null entry.</summary>
        public Gcmf ModelObject
        {
            get
            {
                return modelObjectBackingStorage;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                modelObjectBackingStorage = value;
            }
        }
        
        /// <summary>Create a new GmaEntry with the given name and associated model object.</summary>
        public GmaEntry(string name, Gcmf modelObject)
        {
            Name = name;
            ModelObject = modelObject;
        }

    };
}
