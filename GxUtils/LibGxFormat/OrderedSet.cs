using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat
{
    /// <summary>
    /// A Set class that preserves insertion order for the elements in the set.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set.</typeparam>
    public class OrderedSet<T> : KeyedCollection<T, T>
    {
        protected override T GetKeyForItem(T item)
        {
            return item;
        }
    }
}
