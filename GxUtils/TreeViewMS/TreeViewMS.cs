using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TreeViewMS
{
	/// <summary>
	/// Summary description for TreeViewMS.
	/// </summary>
	public class TreeViewMS : System.Windows.Forms.TreeView
	{
		protected List<TreeNode>     m_coll;
        protected List<Color> m_fore;
        protected List<Color> m_back;
		protected TreeNode		m_lastNode, m_firstNode;

		public TreeViewMS()
		{
			m_coll = new List<TreeNode>();
            m_fore = new List<Color>();
            m_back = new List<Color>();

        }

		protected override void OnPaint(PaintEventArgs pe)
		{
			// TODO: Add custom paint code here

			// Calling the base class OnPaint
			base.OnPaint(pe);
		}


		public List<TreeNode> SelectedNodes
		{
			get
			{
				return m_coll;
			}
			set
			{
				removePaintFromNodes();
				m_coll.Clear();
                m_fore.Clear();
                m_back.Clear();
				m_coll = value;
                for (int i = 0; i < m_coll.Count; i++)
                {
                    TreeNode n = m_coll[i];

                    m_fore.Add(n.ForeColor);
                    m_back.Add(n.BackColor);
                }
                paintSelectedNodes();
			}
		}


// Triggers
//
// (overriden method, and base class called to ensure events are triggered)


		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			base.OnBeforeSelect(e);
				
			bool bControl = (ModifierKeys==Keys.Control);
			bool bShift = (ModifierKeys==Keys.Shift);

			// selecting twice the node while pressing CTRL ?
			if (bControl && m_coll.Contains( e.Node ) )
			{
				// unselect it (let framework know we don't want selection this time)
				e.Cancel = true;
	
				// update nodes
				removePaintFromNodes();
                int index = m_coll.IndexOf(e.Node);
                m_coll.RemoveAt(index);
                m_fore.RemoveAt(index);
                m_back.RemoveAt(index);
                paintSelectedNodes();
				return;
			}

			m_lastNode = e.Node;
			if (!bShift) m_firstNode = e.Node; // store begin of shift sequence
		}

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);

            if (e.Button == MouseButtons.Right)
            {
                // Treat it the same as control click
                // Otherwise the right click menu would break selections
                if (!m_coll.Contains(e.Node))
                {
                    m_coll.Add(e.Node);
                    m_fore.Add(e.Node.ForeColor);
                    m_back.Add(e.Node.BackColor);
                    paintSelectedNodes();
                }
            }
        }


        protected override void OnAfterSelect(TreeViewEventArgs e)
		{
			base.OnAfterSelect(e);

			bool bControl = (ModifierKeys==Keys.Control);
			bool bShift = (ModifierKeys==Keys.Shift);

			if (bControl)
			{
				if ( !m_coll.Contains( e.Node ) ) // new node ?
				{
					m_coll.Add( e.Node );
                    m_fore.Add( e.Node.ForeColor );
                    m_back.Add( e.Node.BackColor );
				}
				else  // not new, remove it from the collection
				{
					removePaintFromNodes();
                    int index = m_coll.IndexOf(e.Node);
					m_coll.RemoveAt(index);
                    m_fore.RemoveAt(index);
                    m_back.RemoveAt(index);
				}
				paintSelectedNodes();
			}
			else 
			{
				// SHIFT is pressed
				if (bShift)
				{
					Queue<TreeNode> myNodeQueue = new Queue<TreeNode>();
                    Queue<Color> myForeQueue = new Queue<Color>();
                    Queue<Color> myBackQueue = new Queue<Color>();

                    TreeNode uppernode = m_firstNode;
					TreeNode bottomnode = e.Node;
					// case 1 : begin and end nodes are parent
					bool bParent = isParent(m_firstNode, e.Node); // is m_firstNode parent (direct or not) of e.Node
					if (!bParent)
					{
						bParent = isParent(bottomnode, uppernode);
						if (bParent) // swap nodes
						{
							TreeNode t = uppernode;
							uppernode = bottomnode;
							bottomnode = t;
						}
					}
					if (bParent)
					{
						TreeNode n = bottomnode;
						while ( n != uppernode.Parent)
						{
                            if (!m_coll.Contains(n)) // new node ?
                            {
                                myNodeQueue.Enqueue(n);
                                myForeQueue.Enqueue(n.ForeColor);
                                myBackQueue.Enqueue(n.BackColor);
                            }

							n = n.Parent;
						}
					}
						// case 2 : nor the begin nor the end node are descendant one another
					else
					{
						if ( (uppernode.Parent==null && bottomnode.Parent==null) || (uppernode.Parent!=null && uppernode.Parent.Nodes.Contains( bottomnode )) ) // are they siblings ?
						{
							int nIndexUpper = uppernode.Index;
							int nIndexBottom = bottomnode.Index;
							if (nIndexBottom < nIndexUpper) // reversed?
							{
								TreeNode t = uppernode;
								uppernode = bottomnode;
								bottomnode = t;
								nIndexUpper = uppernode.Index;
								nIndexBottom = bottomnode.Index;
							}

							TreeNode n = uppernode;
							while (nIndexUpper <= nIndexBottom)
							{
                                if (!m_coll.Contains(n)) // new node ?
                                {
                                    myNodeQueue.Enqueue(n);
                                    myForeQueue.Enqueue(n.ForeColor);
                                    myBackQueue.Enqueue(n.BackColor);
                                }
								
								n = n.NextNode;

								nIndexUpper++;
							} // end while
							
						}
						else
						{
                            if (!m_coll.Contains(uppernode))
                            {
                                myNodeQueue.Enqueue(uppernode);
                                myForeQueue.Enqueue(uppernode.ForeColor);
                                myBackQueue.Enqueue(uppernode.BackColor);
                            }
                            if (!m_coll.Contains(bottomnode))
                            {
                                myNodeQueue.Enqueue(bottomnode);
                                myForeQueue.Enqueue(bottomnode.ForeColor);
                                myBackQueue.Enqueue(bottomnode.BackColor);
                            }
						}
					}

					m_coll.AddRange( myNodeQueue );
                    m_fore.AddRange( myForeQueue );
                    m_back.AddRange( myBackQueue );

					paintSelectedNodes();
					m_firstNode = e.Node; // let us chain several SHIFTs if we like it
				} // end if m_bShift
				else
				{
					// in the case of a simple click, just add this item
					if (m_coll!=null && m_coll.Count>0)
					{
						removePaintFromNodes();
						m_coll.Clear();
                        m_fore.Clear();
                        m_back.Clear();
					}
					m_coll.Add( e.Node );
                    m_fore.Add( e.Node.ForeColor );
                    m_back.Add( e.Node.BackColor );
				}
			}
		}



// Helpers
//
//


		protected bool isParent(TreeNode parentNode, TreeNode childNode)
		{
			if (parentNode==childNode)
				return true;

			TreeNode n = childNode;
			bool bFound = false;
			while (!bFound && n!=null)
			{
				n = n.Parent;
				bFound = (n == parentNode);
			}
			return bFound;
		}

		protected void paintSelectedNodes()
		{
			foreach ( TreeNode n in m_coll )
			{
				n.BackColor = SystemColors.Highlight;
				n.ForeColor = SystemColors.HighlightText;
			}
		}

		protected void removePaintFromNodes()
		{
			if (m_coll.Count==0) return;

			for ( int i = 0; i < m_coll.Count; i++ )
			{
                TreeNode n = m_coll[i];

                n.BackColor = m_back[i];
				n.ForeColor = m_fore[i];
			}

		}

	}
}
