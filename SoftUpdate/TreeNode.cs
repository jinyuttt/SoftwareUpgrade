using System.Collections.Generic;

namespace SoftUpdate
{
    public class TreeNode
    {
        public int ID { get; set; }

        public int NodeID { get; set; }

        public int ParentID { get; set; }

        public string MD5 { get; set; }

        public string Version { get; set; }

        public bool IsDirectory { get; set; }

        public string Name { get; set; }
       
        public List<TreeNode> ChildNodes { get; set; }

        public List<DLLFile> Files { get; set; }

        public TreeNode Parent { get; set; }

        public string Path { get; set; }
        public TreeNode()
        {
            ChildNodes = new List<TreeNode>();
            Files = new List<DLLFile>();
            ParentID = -1;
            NodeID = -1;
        }
       
    }
}