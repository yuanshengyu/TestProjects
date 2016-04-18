using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TreeGenerator
{
    public class TreeNode
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
    }
}
