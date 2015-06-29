using System;

namespace LibGxFormat.Gma
{
    /// <summary>
    /// An entry in a Gma container.
    /// </summary>
    public class GmaEntry
    {
        string name;

        /// <summary>
        /// Name of this Gma model entry
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                name = value;
            }
        }

        Gcmf model;

        /// <summary>
        /// Gcmf model associated with this Gma model entry, or null if this is a null entry.
        /// </summary>
        public Gcmf Model
        {
            get
            {
                return model;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                model = value;
            }
        }
        
        /// <summary>
        /// Create a new GmaEntry with the given name and associated model.
        /// </summary>
        public GmaEntry(string name, Gcmf model)
        {
            Name = name;
            Model = model;
        }
    };
}
