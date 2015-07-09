using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibGxFormat
{
    /// <summary>
    /// A simple non-binary tree implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Tree<T> : Collection<Tree<T>>
    {
        /// <summary>
        /// Get the parent node of the this tree node, or null if it's a root node.
        /// </summary>
        public Tree<T> Parent { get; private set; }

        /// <summary>
        /// Get or set the value associated to this tree node.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Create a new empty tree node with the specified value.
        /// The node will be a root node until it is associated as the child of another tree.
        /// </summary>
        /// <param name="value">The value to associate with this tree node.</param>
        public Tree(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Call the specified function once for each element in the tree, including the current element.
        /// </summary>
        /// <param name="func">The function to call for each tree element.</param>
        public void Traverse(Action<Tree<T>> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            // Call the function for the current node
            func(this);

            // Call the function recursively for all subnodes
            foreach (Tree<T> subNode in Items)
                subNode.Traverse(func);
        }

        #region Manage Parent element when adding/setting/removing items
        protected override void InsertItem(int index, Tree<T> item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.Parent != null)
                throw new ArgumentException("item must not be already associated with another tree.");

            base.InsertItem(index, item);

            // Wait to modify the elements in case the operation fails
            item.Parent = this;
        }

        protected override void ClearItems()
        {
            Tree<T>[] oldItems = Items.ToArray();

            base.ClearItems();

            // Wait to modify the elements in case the operation fails
            foreach (Tree<T> item in oldItems)
                item.Parent = null;
        }

        protected override void SetItem(int index, Tree<T> item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.Parent != null)
                throw new ArgumentException("item must not be already associated with another tree.");
           
            Tree<T> oldItem = (index >= 0 && index < Count) ? this[index] : null; // Let base.SetItem handle bad indexes

            base.SetItem(index, item);

            // Wait to modify the elements in case the operation fails
            oldItem.Parent = null;
            item.Parent = this;
        }

        protected override void RemoveItem(int index)
        {
            Tree<T> oldItem = (index < Count) ? this[index] : null; // Let base.RemoveItem handle bad indexes

            base.RemoveItem(index);

            // Wait to modify the elements in case the operation fails
            oldItem.Parent = null;
        }
        #endregion
    }
}
