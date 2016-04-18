using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;

namespace TreeGenerator
{
    public class Tree
    {
        private List<TreeNode> nodes = new List<TreeNode>();

        public void AddNode(string ID, string ParentID, string Name, string Note, string Description)
        {
            AddNode(ID, ParentID, Name, Note, Description, Color.White);
        }

        public void AddNode(string ID, string ParentID, string Name, string Note, string Description, Color BackColor)
        {
            AddNode(ID, ParentID, Name, Note, Description, BackColor, Color.Black);
        }

        public void AddNode(string ID, string ParentID, string Name, string Note, string Description, Color BackColor, Color ForeColor)
        {
            nodes.Add(new TreeNode { ID = ID, ParentID = ParentID, Name = Name, Note = Note, Description = Description, BackColor = BackColor, ForeColor = ForeColor });
        }

        public TreeNode Find(string ID)
        {
            return nodes.FirstOrDefault(n => n.ID == ID);
        }

        public IEnumerable<TreeNode> Parents(string ID)
        {
            return nodes.Where(n => n.ParentID == ID);
        }

        public TreeNode First
        {
            get { return nodes.FirstOrDefault(); }
        }

        public int Count
        {
            get { return nodes.Count; }
        }
    }
}
