using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using Hikari.Manager;
using System;
using EntityMappingDB;
using System.Threading.Tasks;
using System.Threading;
using ILargerFileStore;

namespace UpdateSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            LoadVersion();
        }

        private List<TreeNode> nodes;
        private List<SoftVersion> softVersions;
        private ViewModel viewModel = null;
        private const string dbseq = "nextval('soft_id_seq')";
        private const string nodeseq = "nextval('node_id_seq')";
        private const string nodeMax = "currval('node_id_seq')";
        private bool isUpdateFile = false;//是否更新了文件
        private bool isClear = false;
        private const long GByte = 1073741824;
        private const long MaxFileSize = 2147483648;//2G

        /// <summary>
        /// 文件删除列表
        /// </summary>
        private List<int> deleteFileList = new List<int>();

        /// <summary>
        /// 删除节点
        /// </summary>
        private List<TreeNode> deleteNodeList = new List<TreeNode>();

        private ILargerStore store=null;

        #region 方法

        /// <summary>
        /// 获取数据
        /// </summary>
        private void LoadVersion()
        {
            softVersions = QueryVersion();
            this.Dispatcher.Invoke(() =>
            {
                CmbVersion.ItemsSource = softVersions;
                if(softVersions.Count==0)
                {
                    TxtVsersion.Text = "1.0.0";
                    ShowTreeView();//主要是为了初始化
                }
                if (string.IsNullOrEmpty(TxtVsersion.Text))
                {
                    //如果为空则是刚刚初始化
                    CmbVersion.SelectedIndex = softVersions.Count - 1;
                }
                else
                {
                    CmbVersion.SelectedIndex = softVersions.FindIndex(x => x.Version == TxtVsersion.Text);
                }
            });
           
         
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            nodes = new List<TreeNode>();
            nodes.Add(new TreeNode() { NodeName = "soft", ParentID = -1, NodeID = 0 });
            //
            nodes.AddRange(QueryNode());
            //

        }

        /// <summary>
        /// 查询节点数据
        /// </summary>
        /// <returns></returns>
        private List<TreeNode> QueryNode()
        {
            string version = "";
            CmbVersion.Dispatcher.Invoke(() =>
            {
                version = CmbVersion.Text;
            });
            var ds = ManagerPool.Singleton.ExecuteQuery("select * from updatesoft where version='" + version + "'"); ;
            var dt = ds.Tables[0];
            dt.TableName = "updatesoft";
            return dt.ToEntityList<TreeNode>();
        }

        /// <summary>
        /// 显示节点树
        /// </summary>
        private void ShowTreeView()
        {
            LoadData();
            Dispatcher.Invoke(() =>
            {
               
                viewModel = new ViewModel("softupdate", new ObservableCollection<TreeNode>(nodes));
                this.DataContext = viewModel;
                if (string.IsNullOrEmpty(TxtVsersion.Text))
                {
                    TxtVsersion.Text = "1.0.0";
                }
            });
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        private void UpdateData()
        {
            ViewModel view = null;

            treeList.Dispatcher.Invoke(() =>
            {
                view = this.treeList.DataContext as ViewModel;
            });
            Thread.Sleep(1000);
            //
            foreach (var root in view.TreeNodes[0].ChildNodes)
            {
                root.ParentID =0;//第一级
                UploadNode(root);
            }
            //存储根节点下面的文件
            foreach(var f  in view.TreeNodes[0].Files)
            {
                UpdateFile(f, view.TreeNodes[0]);
            }
        }

        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private byte[] ReadFile(string file)
        {
            using (StreamReader rd = new StreamReader(file))
            {
                byte[] buf = new byte[rd.BaseStream.Length];
                rd.BaseStream.Read(buf, 0, buf.Length);
                return buf;
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private long FileLen(string file)
        {
            using (FileStream fs = new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                return fs.Length;
            }
        }
        /// <summary>
        /// 传输每个文件
        /// </summary>
        /// <param name="f"></param>
        /// <param name="node"></param>
        private void UpdateFile(DLLFile f,TreeNode node)
        {
            string version = "";
            Dispatcher.Invoke(() =>
            {
                version = TxtVsersion.Text;
            });
            StringBuilder sbr = new StringBuilder();
            string cmd = "";
            //比较
            string cmdCpoy = "";
            if (string.IsNullOrEmpty(f.LocalFile) && version !=node.Version&&f.ID>0)
            {
                //说明该文件已经存在；直接拷贝；
                //发布版本
                cmdCpoy = string.Format("insert into updatesoft(id,nodeid,path,name,md5,parentid,content,version,isdirectory,storeid)   select {0},nodeid,path,name,md5,parentid,content,'{2}',isdirectory,storeid from updatesoft where id={1}", dbseq, f.ID,version);
            }
            else if (!string.IsNullOrEmpty(f.LocalFile))
            {
                //有文件:新增版本，新增文件
                if (!string.IsNullOrEmpty(f.MD5))
                {
                    //说明是有文件的；比较文件变化，覆盖文件
                    string curv = MD5.GetMD5HashFromFile(f.LocalFile);
                    if (f.MD5 == curv)
                    {
                        if (version != node.Version)
                        {
                            //直接,发布新版本拷贝
                            cmdCpoy = string.Format("insert into updatesoft(id,nodeid,path,name,md5,parentid,content,version,isdirectory,storeid)   select {0},nodeid,path,name,md5,parentid,content,'{2}',isdirectory,storeid from updatesoft where id={1}", dbseq, f.ID,version);
                        }
                        else
                        {
                            //说明该文件既没有参与发布新版本，也不需要修改，直接不操作
                            //上传文件选中有变化的，没有变化的不处理
                            return;
                        }
                    }
                    else
                    {
                        f.MD5 = curv;//文件已经变化
                    }
                }
                else
                {
                    //完全新增
                    f.MD5=MD5.GetMD5HashFromFile(f.LocalFile);
                }
            }
            else if (string.IsNullOrEmpty(f.LocalFile))
            {
                //说明是不需要操作文件,保持不动
                return;
              
            }
            //
            if (string.IsNullOrEmpty(cmdCpoy))
            {
                //新增
                cmd = "insert into updatesoft(id,nodeid,path,name,md5,parentid,content,version,isdirectory,storeid)values(";
                cmd += "@id,@nodeid,@path,@name,@md5,@parentid,@content,@version,@isdirectory,@storeid)";
                sbr.Length = 0;
                byte[] fbyte = null;
                long size = FileLen(f.LocalFile);
                int storeid = -1;
                if(size<MaxFileSize)
                {
                    fbyte = ReadFile(f.LocalFile);
                }
                else
                {
                    //检测已经存在的文件是否有
                    storeid = QueryStoreID(f);
                    if(storeid<0)
                    {
                        //则需要提交
                        storeid = QuerySeqId();
                        string remoteName = "";
                        string remotePath= store.CommitFile(storeid,f.LocalFile,out remoteName);
                        LagreFile(storeid, remotePath, f.MD5, remoteName, f.Name);
                    }
                }
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    ["id"] = QuerySeqId(),
                    ["nodeid"] = QueryNodeId(),
                    ["path"] = f.Name,
                    ["name"] = f.Name,
                    ["md5"] = f.MD5,
                    ["parentid"] = node.NodeID,
                    ["content"] = fbyte,
                    ["version"] = version,
                    ["isdirectory"] = false,
                    ["storeid"] = storeid
                };
                CmdExecute(cmd,dic);
                sbr.Length = 0;
                isUpdateFile = true;
            }
            else
            {
                CmdExecute(cmdCpoy);

            }
        }

        /// <summary>
        /// 处理一个节点（目录）
        /// </summary>
        /// <param name="node"></param>
        private void UploadNode(TreeNode node)
        {
            if(node.NodeName=="soft")
            {
                return;
            }
            StringBuilder sbr = new StringBuilder();
            string version = "";
            TxtVsersion.Dispatcher.Invoke(() =>
            {
                version = TxtVsersion.Text;
            });
            if (node.ID < 1 || version != node.Version)
            {
               //没有该节点或者发布新版本才提交
                string cmd = "insert into updatesoft(id,nodeid,path,name,parentid,version,isdirectory)values(";
                sbr.Length = 0;
                sbr.Append(cmd);
                sbr.AppendFormat("{0},", dbseq);
                if (node.NodeID == -1)
                {
                    sbr.AppendFormat("{0},", nodeseq);
                }
                else
                {
                    sbr.AppendFormat("{0},", node.NodeID);
                }
                sbr.AppendFormat("'{0}',", node.NodeName);
                sbr.AppendFormat("'{0}',", node.NodeName);
                sbr.AppendFormat("{0},", node.ParentID);
                sbr.AppendFormat("'{0}',", version);
                sbr.AppendFormat("{0})", true);
                CmdExecute(sbr.ToString(), null);//添加目录节点
            }
            foreach (var f in node.Files)
            {
                if(node.NodeID==-1)
                {
                    //获取刚刚使用的ID
                    node.NodeID = QueryCureentNodeId();
                }
                UpdateFile(f, node);
            }
            //
            foreach(var child in node.ChildNodes)
            {
               if(child.ParentID==-1)
                {
                    child.ParentID = node.NodeID;
                }
                UploadNode(child);
            }
        }


        /// <summary>
        /// 更新数据库版本信息
        /// </summary>
        private void UpdateVersion()
        {
            string sql = "";
            string cur = "";
            TxtVsersion.Dispatcher.Invoke(() =>
            {
                cur= TxtVsersion.Text;
            });
            
            var result=softVersions.Where(x => x.Version == cur);
            if (result.Count()==0)
            {
                sql = string.Format("insert into softvserion(id,version,vertime,versionid,clear)values({0},'{1}','{2}',{3},{4})", dbseq, cur, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 1, isClear);
            }
            else
            {
                var r = result.Last();
                if (r == null)
                {
                    //发布新版本
                    sql = string.Format("insert into softvserion(id,version,vertime,versionid,clear)values({0},'{1}','{2}',{3},{4})", dbseq, cur, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 1, isClear);
                }
                else
                {
                    //修复个别文件，不作为版本
                    if (isUpdateFile)
                    {
                        //必须是有新文件才修改
                        sql = string.Format("update softvserion set versionid=versionid+1,clear={0} where id={1}", isClear, r.ID);
                    }
                }
            }
            if(!string.IsNullOrEmpty(sql))
            {
              int r=   ManagerPool.Singleton.ExecuteUpdate(sql);
                isUpdateFile = false;
                if(r<1)
                {
                    Console.WriteLine("参数");
                }
            }
           
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        private void CmdExecute(string sql,Dictionary<string,object> param=null)
        {
            ManagerPool.Singleton.ExecuteUpdate(sql,null,param);
        }

        /// <summary>
        /// 查询当前树节点id
        /// </summary>
        /// <returns></returns>
        private int QueryCureentNodeId()
        {
             string maxNode = "select " + nodeMax;
             object v=  ManagerPool.Singleton.ExecuteScalar(maxNode);
            if(v==null)
            {
                return -1;
            }
            return Convert.ToInt32(v);
        }

        /// <summary>
        /// 查询下一个节点ID
        /// </summary>
        /// <returns></returns>
        private int QueryNodeId()
        {
            string maxNode = "select " +nodeseq;
            object v = ManagerPool.Singleton.ExecuteScalar(maxNode);
            if (v == null)
            {
                return -1;
            }
            return Convert.ToInt32(v);
        }

        /// <summary>
        /// 查询是否已经存在文件
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private int QueryStoreID(DLLFile f)
        {
            string sql = string.Format("select id from largestore where localname='{0}' and md5='{1}'",
                                   f.Name, f.MD5);
           object v= ManagerPool.Singleton.ExecuteScalar(sql);
            if (v == null)
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(v);
            }

        }

        /// <summary>
        /// 查询主键值
        /// </summary>
        /// <returns></returns>
        private int QuerySeqId()
        {
           
            object v = ManagerPool.Singleton.ExecuteScalar("select " + dbseq);
            if (v == null)
            {
                return -1;
            }
            return Convert.ToInt32(v);
        }

        /// <summary>
        /// 查询所有版本信息
        /// </summary>
        /// <returns></returns>
        private List<SoftVersion> QueryVersion()
        {
            var ds=  ManagerPool.Singleton.ExecuteQuery("select * from softvserion");
            var dt = ds.Tables[0];
            dt.TableName = "softvserion";
            return dt.ToEntityList<SoftVersion>();
        }

        /// <summary>
        /// 添加节点
       /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        private void AddDirectory(string path,TreeNode node)
        {
            DirectoryInfo info = new DirectoryInfo(path);
             var  dirs= info.GetDirectories();
            string filter = TxtFilter.Text;
            if (dirs.Length > 0)
            {
                foreach (var sub in dirs)
                {
                    TreeNode child = new TreeNode
                    {
                        NodeName = sub.Name
                    };
                    node.ChildNodes.Add(child);
                    AddDirectory(sub.FullName, child);
                    //
                    var files = sub.GetFiles();
                    foreach (var f in files)
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            if (f.Extension == filter)
                            {
                                continue;
                            }
                        }
                        DLLFile dLLFile = new DLLFile() { LocalFile = f.FullName, Name = f.Name };
                        child.Files.Add(dLLFile);
                    }
                }
                //
                var cur = info.GetFiles();
                foreach (var f in cur)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (f.Extension == filter)
                        {
                            continue;
                        }
                    }
                    DLLFile dLLFile = new DLLFile() { LocalFile = f.FullName, Name = f.Name };
                    node.Files.Add(dLLFile);
                }
            }
            else
            {
                var files = info.GetFiles();
                foreach (var f in files)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (f.Extension == filter)
                        {
                            continue;
                        }
                    }
                    DLLFile dLLFile = new DLLFile() { LocalFile = f.FullName, Name = f.Name };
                    node.Files.Add(dLLFile);
                }
            }
        }
      
        /// <summary>
        /// 查找父节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private  TreeNode FindParent(TreeNode node)
        {
            ViewModel view = treeList.DataContext as ViewModel;
            TreeNode treeNode = null;
            foreach(var  child in view.TreeNodes )
            {
                treeNode= FindParent(child, node);
                if(treeNode!=null)
                {
                    break;
                }
            }
            return treeNode;
        }

        /// <summary>
        /// 查找父节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeNode FindParent(TreeNode parent,  TreeNode node)
        {
            foreach (var child in parent.ChildNodes)
            {
                //查找一遍子节点
                if (child == node)
                {
                    return parent;
                }
            }
            //再逐个查找子节点是否符合
            TreeNode treeNode = null;
            foreach (var child in parent.ChildNodes)
            {
               treeNode= FindParent(child, node);
                if(treeNode!=null)
                {
                    return treeNode;
                }
            }
            return null;
        }


        /// <summary>
        /// 重置数据库版本信息
        /// </summary>
        private void  Reset()
        {
            string sql = "TRUNCATE TABLE updatesoft";
            ManagerPool.Singleton.ExecuteUpdate(sql);
            ManagerPool.Singleton.ExecuteUpdate("TRUNCATE TABLE softvserion");
            ManagerPool.Singleton.ExecuteUpdate("alter sequence node_id_seq restart with 1");
            LoadVersion();
            ShowTreeView();
            isClear = true;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        private void DeleteFiles()
        {
            StringBuilder sbr = new StringBuilder();
            string sql = "delete from updatesoft where id in(";
            for (int i = 0; i < deleteFileList.Count; i++)
            {
                sbr.AppendFormat("{0},", deleteFileList[i]);
                if (i % 20 == 0 && i > 0)
                {
                    string tmp = sql + sbr.ToString();
                    tmp = tmp.Remove(tmp.Length - 1) + ")";
                    ManagerPool.Singleton.ExecuteUpdate(tmp);
                    sbr.Length = 0;
                }
            }
            if (sbr.Length > 0)
            {
                string tmp = sql + sbr.ToString();
                tmp = tmp.Remove(tmp.Length - 1) + ")";
                ManagerPool.Singleton.ExecuteUpdate(tmp);
                sbr.Length = 0;
            }
            deleteFileList.Clear();
        }

        /// <summary>
        /// 删除节点信息
        /// </summary>
        private void DeleteNode()
        {
            List<int> lst = new List<int>();
            foreach(var node in deleteNodeList)
            {
                DeleteNode(node, lst);
            }
            if(lst.Count>0)
            {
                StringBuilder sbr = new StringBuilder();
                string sql = "delete from updatesoft where id in(";
                for (int i = 0; i < lst.Count; i++)
                {
                    sbr.AppendFormat("{0},", lst[i]);
                    if (i % 20 == 0 && i > 0)
                    {
                        string tmp = sql + sbr.ToString();
                        tmp = tmp.Remove(tmp.Length - 1) + ")";
                        ManagerPool.Singleton.ExecuteUpdate(tmp);
                        sbr.Length = 0;
                    }
                }
                if (sbr.Length > 0)
                {
                    string tmp = sql + sbr.ToString();
                    tmp = tmp.Remove(tmp.Length - 1) + ")";
                    ManagerPool.Singleton.ExecuteUpdate(tmp);
                    sbr.Length = 0;
                }
            }
            deleteNodeList.Clear();
        }

        private void DeleteNode(TreeNode node,List<int> lst)
        {
            foreach(var f in node.Files)
            {
                if (f.ID > 0)
                {
                    lst.Add(f.ID);
                }
            }
            foreach(var n in node.ChildNodes)
            {
                if(node.ID>0)
                {
                    lst.Add(node.ID);
                    DeleteNode(n, lst);
                }
               
            }
        }

        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="path"></param>
        /// <param name="md5"></param>
        /// <param name="remoteName"></param>
        /// <param name="localName"></param>
        private void LagreFile(int id,string path,string md5,string remoteName,string localName)
        {
            string sql = string.Format("insert into largestore(id,path,md5,name,localname) values({0},{1},{2},{3},{4},{5})",
                id, path, md5, remoteName, localName);
            ManagerPool.Singleton.ExecuteUpdate(sql);
        }
        #endregion

        private void BtnDirectory_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.Description = "pls select a folder";
            string filter = TxtFilter.Text;
            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                DirectoryInfo info = new DirectoryInfo(path);
                TreeNode node = treeList.SelectedItem as TreeNode;
                if(node!=null)
                {

                    foreach (var dir in node.ChildNodes)
                    {
                        if(info.Name==dir.NodeName)
                        {
                           System.Windows.MessageBox.Show("已经存在该目录");
                            return;
                        }
                    }
                    //
                    var child = new TreeNode() { NodeName = info.Name };
                    foreach (var f in info.GetFiles())
                    {
                        if(!string.IsNullOrEmpty(filter))
                        {
                            if(f.Extension==filter)
                            {
                                continue;
                            }
                        }
                        DLLFile dLLFile = new DLLFile() { LocalFile = f.FullName, Name = f.Name };
                        child.Files.Add(dLLFile);
                    }
                    node.ChildNodes.Add(child);
                }
            }
        }

        private void BtnFiles_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog openFileDialog = new WinForms.OpenFileDialog();
            string fliter = TxtFilter.Text;
            if (openFileDialog.ShowDialog() == WinForms.DialogResult.OK)//注意，此处一定要手动引入System.Window.Forms空间，否则你如果使用默认的DialogResult会发现没有OK属性
            {
                string[] path = openFileDialog.FileNames;
                TreeNode node = treeList.SelectedItem as TreeNode;
                if (node != null)
                {
                    foreach (string f in path)
                    {
                       if(!string.IsNullOrEmpty(fliter))
                        {
                            if(f.EndsWith(fliter))
                            {
                                continue;
                            }
                        }
                        FileInfo fileInfo = new FileInfo(f);
                        DLLFile dLLFile = new DLLFile() { LocalFile = fileInfo.FullName, Name = fileInfo.Name };
                        node.Files.Add(dLLFile);
                    }
                }
            }
        }

        private void TreeList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeNode v =  e.NewValue as TreeNode;
            v.Parent = e.OldValue as TreeNode;
            if (v!=null)
            {
                gridData.ItemsSource = v.Files;
            }
        }

        private void BtnChildFiles_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.Description = "pls select a folder";
            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;
               
                TreeNode node = treeList.SelectedItem as TreeNode;
                if (node != null)
                {
                    //
                    AddDirectory(path, node);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.Source as MenuItem;
            var treeItem= treeList.SelectedItem as TreeNode;
            if (item.Header.ToString()=="删除")
            {
                if (treeItem.Parent != null)
                {
                    treeItem.Parent.ChildNodes.Remove(treeItem);
                }
                else
                {
                    TreeNode p = FindParent(treeItem);
                    if (p != null)
                    {
                        p.ChildNodes.Remove(treeItem);
                    }
                }
                if (treeItem.ID > 0)
                {
                    deleteNodeList.Add(treeItem);
                }
            }
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {

            WaitLoading waitLoading = new WaitLoading();
            waitLoading.Owner = this;
            Task.Factory.StartNew(() =>
            {
                DeleteFiles();
                DeleteNode();
                UpdateData();
                UpdateVersion();
                LoadVersion();
                if (store != null)
                {
                    while (!store.GetCommitComplete())
                    {
                        Thread.Sleep(2000);
                    }
                }
                Thread.Sleep(1000);
                Dispatcher.Invoke(() =>
                {
                    waitLoading.Close();
                    MessageBox.Show("OK");
                });

            });
            waitLoading.ShowDialog();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void CmbVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           if(e.RemovedItems.Count>0&&e.AddedItems.Count>0&&e.AddedItems[0]==e.RemovedItems[0])
            {
                return;
            }
            Task.Factory.StartNew(() =>
            {
                
                ShowTreeView();
            });

        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            nodes = new List<TreeNode>();
            nodes.Add(new TreeNode() { NodeName = "soft", ParentID = -1, NodeID = 0 });
            ViewModel view = new ViewModel("softupdate", new ObservableCollection<TreeNode>(nodes));
            this.DataContext = view;
        }

        private void MenuGridDel_Click(object sender, RoutedEventArgs e)
        {
        
            ObservableCollection<DLLFile>   vf= gridData.ItemsSource as ObservableCollection<DLLFile>;
            DLLFile dll = gridData.SelectedItem as DLLFile;
            vf.Remove(dll);
            if(dll.ID>0)
            {
                deleteFileList.Add(dll.ID);
            }
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          
            if (treeList.Items.Count > 0)
            {
                TreeViewItem item = (TreeViewItem)(treeList.ItemContainerGenerator.ContainerFromIndex(0));
                item.IsExpanded = true;
                item.IsSelected = true;
            }
            store = StoreFactory.GetStore();
        }
    }
}
