using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GxModelViewer_WinFormsExt
{
    /// <summary>
    /// A TreeView in which the nodes can have three states: Checked, Unchecked and Partially Checked.
    /// Nodes are managed automatically:
    /// - If a node has all its child checked, it is automatically checked
    /// - If a node has all its child unchecked, it is automatically unchecked
    /// - Otherwise, it is partially checked.
    /// 
    /// This method is completely independent of the TreeView.Checkboxes and TreeNode.Checked property.
    /// No events related to those will be triggered.
    /// It's a good idea to set TreeView.Checkboxes to false in order to avoid two sets of checkboxes.
    /// 
    /// Since we aren't getting much help from TreeView/TreeNode to implement this feature,
    /// you need to obey some simple rules in the user code. Those are:
    /// - In order to get the checked state of a tree node, use the GetTreeNodeCheckState static method.
    /// - In order to set the checked state of a tree node, use the SetTreeNodeCheckState static method.
    /// 
    /// Be careful about leaving tree nodes in an inconsistent state, for example, by calling SetTreeNodeCheckState,
    /// then adding more child nodes without calling SetTreeNodeCheckState on them.
    /// Preferably, create your whole tree structure, then call SetTreeNodeCheckState on the nodes you wish.
    /// The general rule is that your calls to SetTreeNodeCheckState must affect every node in the tree,
    /// so for example, call it on every root node of the tree, or in every leaf node of the tree.
    /// 
    /// A custom AfterCheckState event will be fired when a node CheckState changes by mouse or keyboard action,
    /// or by calling the SetCheckState method, only on the single node on which the action has been taken -
    /// - NOT on all nodes whose CheckState is affected by the action!
    /// </summary>
    public class TreeViewAutoPartialCheckBox : TreeViewMS.TreeViewMS
    {
        /// <summary>
        /// Called after a mouse or keyboard action causes the CheckState action of the affected node to change.
        /// </summary>
        public event TreeViewEventHandler AfterCheckState;

        /// <summary>
        /// Create a new empty TreeViewAutoPartialCheckBox.
        /// </summary>
        public TreeViewAutoPartialCheckBox()
        {
            // Create an image list with the various check box state images
            // Instead of showing the check boxes with the CheckBoxes property,
            // we will "simulate" them by using the StateImageList property
            StateImageList = new ImageList();

            foreach (CheckState checkState in Enum.GetValues(typeof(CheckState)))
            {
                // Map the CheckState to the CheckBoxState used by CheckBoxRenderer
                CheckBoxState checkBoxState;
                switch (checkState)
                {
                    case CheckState.Unchecked: checkBoxState = CheckBoxState.UncheckedNormal; break;
                    case CheckState.Checked: checkBoxState = CheckBoxState.CheckedNormal; break;
                    case CheckState.Indeterminate: checkBoxState = CheckBoxState.MixedNormal; break;
                    default: throw new InvalidOperationException("Unreachable code.");
                }

                // Draw the check box to a bitmap and add it to the state image list
                Bitmap checkBoxImage = new Bitmap(StateImageList.ImageSize.Width, StateImageList.ImageSize.Height);
                using (Graphics checkBoxGraphics = Graphics.FromImage(checkBoxImage))
                    CheckBoxRenderer.DrawCheckBox(checkBoxGraphics, Point.Empty, checkBoxState);
                StateImageList.Images.Add(checkBoxImage);
            }
        }

        /// <summary>Get the check state of a tree node.</summary>
        /// <param name="node">The node from which the check state has to be obtained.</param>
        /// <returns>The check state of the tree node.</returns>
        public CheckState GetCheckState(TreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return (CheckState)node.StateImageIndex;
        }

        /// <summary>Set the check state of a tree node.</summary>
        /// <param name="node">The node whose state has to be set.</param>
        /// <param name="newState">The new check state state to set.</param>
        public void SetCheckState(TreeNode node, CheckState newState)
        {
            SetCheckStateImpl(node, newState, TreeViewAction.Unknown);
        }

        private void SetCheckStateImpl(TreeNode node, CheckState newState, TreeViewAction action)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (newState != CheckState.Checked && newState != CheckState.Unchecked)
                throw new ArgumentOutOfRangeException("newState", "Only checked and unchecked states can be set.");

            // Set the check state and update the children and parent
            PropagateNodeCheckStateDown(node, newState);
            PropagateNodeCheckStateUp(node);

            // Trigger the AfterCheckState event on the affected node
            if (AfterCheckState != null)
                AfterCheckState(this, new TreeViewEventArgs(node, action));
        }

        /// <summary>Toggles the CheckState of the specified node on response of a click/keyboard event.</summary>
        /// <param name="node">The node affected by the event.</param>
        /// <param name="action">The action that caused the event.</param>
        private void ToggleNodeCheckState(TreeNode node, TreeViewAction action)
        {
            // Compute the new check box state of the node
            CheckState newState;
            switch ((CheckState)node.StateImageIndex)
            {
                case CheckState.Unchecked: newState = CheckState.Checked; break;
                case CheckState.Checked: newState = CheckState.Unchecked; break;
                case CheckState.Indeterminate: newState = CheckState.Checked; break; // Standard behaviour
                default: throw new InvalidOperationException("Invalid TreeNode.StateImageIndex - Check your code.");
            }

            // Update the checked state of the node
            SetCheckStateImpl(node, newState, action);
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);

            // Find out if the user clicked on the state image, so we need to toggle the CheckState
            TreeViewHitTestInfo hitTestInfo = HitTest(e.Location);
            if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                ToggleNodeCheckState(hitTestInfo.Node, TreeViewAction.ByMouse);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Find out if the user pressed space on the selected node, so we need to toggle the CheckState
            if (SelectedNode != null && e.KeyCode == Keys.Space)
                ToggleNodeCheckState(SelectedNode, TreeViewAction.ByKeyboard);
        }

        /// <summary>Propagates a CheckState change to the specified node and to all its children recursively.</summary>
        /// <param name="node">The node whose CheckState has to be set.</param>
        /// <param name="newState">The new node CheckState.</param>
        private static void PropagateNodeCheckStateDown(TreeNode node, CheckState newState)
        {
            // Set the StateImageIndex and Checked properties of the current node
            node.StateImageIndex = (int)newState;

            // Copy the state of the node to all its children recursively
            foreach (TreeNode subNode in node.Nodes)
                PropagateNodeCheckStateDown(subNode, newState);
        }

        /// <summary>Propagates a CheckState change to the parents of the specified node recursively.</summary>
        /// <param name="node">The node whose CheckState has been set.</param>
        private static void PropagateNodeCheckStateUp(TreeNode node)
        {
            // Nothing to do if this was a root node
            if (node.Parent == null)
                return;

            // Calculate the new state of parent according to its children
            // (See description of the behaviour at the top of the class)
            CheckState parentCheckState;
            if (node.Parent.Nodes.Cast<TreeNode>().All(neighborNode => neighborNode.StateImageIndex == node.StateImageIndex))
                parentCheckState = (CheckState)node.StateImageIndex;
            else
                parentCheckState = CheckState.Indeterminate;

            // Set the StateImageIndex and Checked properties of the parent node
            node.Parent.StateImageIndex = (int)parentCheckState;

            // Update all parents recursively
            PropagateNodeCheckStateUp(node.Parent);
        }
    }
}
