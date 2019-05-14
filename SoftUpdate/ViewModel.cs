#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UpdateSoft
* 项目描述 ：
* 类 名 称 ：ViewModel
* 类 描 述 ：
* 命名空间 ：UpdateSoft
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion

using System.Collections.Generic;
using System.Linq;

namespace SoftUpdate
{

    /* ============================================================================== 
* 功能描述：ViewModel 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class ViewModel 
    {
       
       
        private List<TreeNode> treenodes = new List<TreeNode>();
        private string name;
       public string Name
        {
            get { return name; }
            set { name = value;
               
            }
        }
        public List<TreeNode> TreeNodes
        {
            get { return treenodes; }
            set
            {
                treenodes = value;
            }
        }

        public ViewModel(string name, List<TreeNode> Nodes)
        {
            this.name = name;
            // Nodes是我已经获得的一组节点
            TreeNodes = GetChildNodes(-1, Nodes);
        }

        
        private List<TreeNode> GetChildNodes(int parentID, List<TreeNode> nodes)
        {
            List<TreeNode> mainNodes =new List<TreeNode>(nodes.Where(x => x.ParentID == parentID).ToList());
            List<TreeNode> otherNodes =new List<TreeNode>(nodes.Where(x => x.ParentID != parentID).ToList());
            foreach (TreeNode node in mainNodes)
            {
                var child = GetChildNodes(node.NodeID, otherNodes);
                node.ChildNodes = new List<TreeNode>(child.Where(x => x.IsDirectory));
                var files = child.Where(x => !x.IsDirectory);
                foreach (var f in files)
                {
                    DLLFile dLLFile = new DLLFile() { DLLPath = f.Name, ID = f.ID, MD5 = f.MD5, Name = f.Name, NodeID = f.NodeID, ParentID = f.ParentID, Version = f.Version };
                    node.Files.Add(dLLFile);
                }
            }
            return mainNodes;
        }
    }
}
