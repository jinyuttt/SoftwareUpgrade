using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UpdateSoft
{
    public class TreeNode: INotifyPropertyChanged
    {
        public int ID { get; set; }

        public int NodeID { get; set; }

        public int ParentID { get; set; }

        public string MD5 { get; set; }

        public int SoreID { get; set; }
        public string Version { get; set; }
        public bool IsDirectory { get; set; }

        [EntityMappingDB.DataField("Name")]
        public string NodeName { get; set; }
        //public bool IsExpanded { get; set; } // 节点是否展开
        //public bool IsSelected { get; set; } // 节点是否选中
        public ObservableCollection<TreeNode> ChildNodes { get; set; }

        public ObservableCollection<DLLFile> Files { get; set; }

        public TreeNode Parent { get; set; }
        public TreeNode()
        {
            ChildNodes = new ObservableCollection<TreeNode>();
            Files = new ObservableCollection<DLLFile>();
            ParentID = -1;
            NodeID = -1;
        }
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}