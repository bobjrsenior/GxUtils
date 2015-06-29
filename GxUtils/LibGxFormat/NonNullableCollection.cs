using System;
using System.Collections.ObjectModel;

namespace LibGxFormat
{
    /// <summary>
    /// Base class for a collection that does not accept null values.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public class NonNullableCollection<T> : Collection<T> where T : class
    {
        protected override void InsertItem(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException();

            base.SetItem(index, item);
        }
    }
}
