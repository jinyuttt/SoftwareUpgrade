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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace UpdateSoft
{

    /* ============================================================================== 
* 功能描述：ViewModel 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class ViewModel : INotifyPropertyChanged
    {
       
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<TreeNode> treenodes = new ObservableCollection<TreeNode>();
        private string name;
       public string Name
        {
            get { return name; }
            set { name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }
        public ObservableCollection<TreeNode> TreeNodes
        {
            get { return treenodes; }
            set
            {
                treenodes = value;

                OnPropertyChanged(new PropertyChangedEventArgs("ChildNodes"));
            }
        }

        public ViewModel(string name,ObservableCollection<TreeNode> Nodes)
        {
            this.name = name;
            // Nodes是我已经获得的一组节点
            TreeNodes = GetChildNodes(-1, Nodes);
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if(PropertyChanged!=null)
            {
                PropertyChanged(this, e);
            }
        }
        private ObservableCollection<TreeNode> GetChildNodes(int parentID, ObservableCollection<TreeNode> nodes)
        {
            ObservableCollection<TreeNode> mainNodes =new ObservableCollection<TreeNode>(nodes.Where(x => x.ParentID == parentID).ToList());
            ObservableCollection<TreeNode> otherNodes =new ObservableCollection<TreeNode>(nodes.Where(x => x.ParentID != parentID).ToList());
            foreach (TreeNode node in mainNodes)
            {
                var child = GetChildNodes(node.NodeID, otherNodes);
                node.ChildNodes = new ObservableCollection<TreeNode>(child.Where(x => x.IsDirectory));
                var files = child.Where(x => !x.IsDirectory);
                foreach(var f in files)
                {
                    DLLFile dLLFile = new DLLFile() { DLLPath = f.NodeName, ID = f.ID, MD5 = f.MD5, Name = f.NodeName, NodeID = f.NodeID, ParentID = f.ParentID, Version = f.Version, StoreID=f.SoreID };
                    node.Files.Add(dLLFile);
                }
            }
            return mainNodes;
        }
    }
}
