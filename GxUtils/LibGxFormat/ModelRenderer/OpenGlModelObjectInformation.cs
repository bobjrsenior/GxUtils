using System;

namespace LibGxFormat.ModelRenderer
{
    /// <summary>
    /// Data structure for retorning information about objects in a model rendered using OpenGL.
    /// </summary>
    public class OpenGlModelObjectInformation
    {
        /// <summary>
        /// Get the name associated with this model object.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get the display list associated with this model object.
        /// </summary>
        public int DisplayListIndex { get; private set; }
        
        /// <summary>Create a new empty OpenGlModelObjectTree from the given display list index.</summary>
        /// <param name="displayListIndex">The OpenGL display list index.</param>
        public OpenGlModelObjectInformation(string name, int displayListIndex)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.Name = name;
            this.DisplayListIndex = displayListIndex;
        }
    }
}
