#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：SoftUpdate
* 项目描述 ：
* 类 名 称 ：SoftList
* 类 描 述 ：
* 命名空间 ：SoftUpdate
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion

using EntityMappingDB;
using Hikari.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ILargerFileStore;
using System.Threading;

namespace SoftUpdate
{

    /* ============================================================================== 
* 功能描述：SoftList 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class SoftList
    {
        private readonly List<Task> tasks = new List<Task>();

       

        /// <summary>
        /// 控制的版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 当前使用的版本
        /// </summary>
        public string CurVersion { get; set; }

        /// <summary>
        /// 当前版本ID
        /// </summary>
        public int MaxVsersionID { get; set; }

        /// <summary>
        /// 返回版本信息，说明处理过
        /// </summary>
        public SoftVersion LastVersion { get; set; }
        /// <summary>
        /// 软件根目录
        /// </summary>
        public string CureentPath { get; set; }

        private const string TmpFile = ".tmp.o";

        /// <summary>
        /// 是否需要文件名称重置
        /// </summary>
        public bool FileRename { get; set; }

        /// <summary>
        /// 查询所有版本
        /// </summary>
        /// <returns></returns>
        private List<SoftVersion> QueryVersion()
        {
            var ds = ManagerPool.Singleton.ExecuteQuery("select * from softvserion");
            var dt = ds.Tables[0];
            dt.TableName = "softvserion";
            return dt.ToEntityList<SoftVersion>();
        }

        /// <summary>
        /// 获取最新
        /// </summary>
        /// <returns></returns>
        private List<SoftVersion> GetLatest()
        {
            var ds= ManagerPool.Singleton.ExecuteQuery("select * from softvserion where version in (select max(version) from softvserion)");
           // var ds = ManagerPool.Singleton.ExecuteQuery("select * from softvserion where version=max(version)");
            var dt = ds.Tables[0];
            dt.TableName = "softvserion";
            return dt.ToEntityList<SoftVersion>();
        }

        /// <summary>
        /// 清除全部
        /// </summary>
        private void Clear()
        {
            DirectoryInfo info = new DirectoryInfo(CureentPath);
            if(!info.Exists)
            {
                return;
            }
            var all = info.GetDirectories();
            foreach (var directory in all)
            {
                try
                {
                    directory.Delete(true);
                }
                catch
                {

                }
            }
            var files = info.GetFiles();
            foreach (var f in files)
            {
                try
                {
                    f.Delete();
                }
                catch
                {
                    //说明有文件正在使用
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        private ViewModel LoadData(SoftVersion softVersion)
        {
            var nodes = new List<TreeNode>
            {
                new TreeNode() { Name = "soft", ParentID = -1, NodeID = 0 }
            };
            //
            nodes.AddRange(QueryNode(softVersion));
            //
            return new ViewModel(CurVersion, nodes);
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <returns></returns>
        private List<TreeNode> QueryNode(SoftVersion softVersion)
        {
            var ds = ManagerPool.Singleton.ExecuteQuery("select * from updatesoft where version='" + softVersion.Version + "'"); ;
            var dt = ds.Tables[0];
            dt.TableName = "updatesoft";
            return dt.ToEntityList<TreeNode>();
        }

        /// <summary>
        /// 保持文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="id"></param>
        private void SaveFile(string path,int id)
        {
            FileInfo fileInfo = new FileInfo(path);
            bool isTmp = false;
            if(fileInfo.Exists)
            {
                try
                {
                    File.Delete(path);
                }
                catch(Exception ex)
                {
                    //正常情况该文件正在使用
                    path = path + TmpFile;
                    isTmp = true;
                }
            }
             var obj=  ManagerPool.Singleton.ExecuteScalar("select content from updatesoft where id=" + id);
             byte[] v = obj as byte[];
            if (v != null)
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fs.Write(v, 0, v.Length);
                }
            }
            if(isTmp)
            {
                //文件名称改成后缀.tmp,表明文件写入完成，需要另外一个程序来修改
                FileInfo finfo = new FileInfo(path);
                finfo.MoveTo(path.Remove(path.Length-2));
                FileRename = true;
            }
        }


        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="id"></param>
        private void GetRemoteFile(int id)
        {
            string sql = "select path,name from largestore where id=" + id;
            var ds= ManagerPool.Singleton.ExecuteQuery(sql);
            ILargerStore store = StoreFactory.GetStore();
            if (ds != null)
            {
                var dt = ds.Tables[0];
                 var lst= dt.ToEntityList<LargeStore>();
                if (lst != null && lst.Count > 0&&store!=null)
                {
                    store.DownloadFile(lst[0]);
                }
            }
        }


        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="node"></param>
        private void UpdownFile(TreeNode node)
        {
            string md5 = "";
            //遍历文件
            if (string.IsNullOrEmpty(node.Path))
            {
                node.Path = CureentPath;
            }
            if (!Directory.Exists(node.Path))
            {
                Directory.CreateDirectory(node.Path);
            }
            var task = Task.Factory.StartNew(() =>
              {
                  foreach (var f in node.Files)
                  {
                    //比较或者复制
                      string filePath = Path.Combine(node.Path, f.Name);
                      f.DLLPath = filePath;
                      if (File.Exists(filePath))
                      {
                        //存在则比较
                          md5 = MD5.GetMD5HashFromFile(filePath);
                          if (md5 == f.MD5)
                          {
                              continue;
                          }
                      }
                      if (f.StoreID > 0)
                      {
                          //说明是大文件存储
                          GetRemoteFile(f.StoreID);
                      }
                      else
                      {
                          SaveFile(filePath, f.ID);
                      }
                  }
              });
            tasks.Add(task);
            foreach (var dir in node.ChildNodes)
            {
                if (string.IsNullOrEmpty(dir.Path))
                {
                    dir.Path = Path.Combine(node.Path, dir.Name);
                }
                UpdownFile(dir);
            }
        }
         
        /// <summary>
        /// 开始下载
        /// </summary>
        private void Updown(SoftVersion softVersion)
        {
            DirectoryInfo info = new DirectoryInfo(CureentPath);
            if (!info.Exists)
            {
                info.Create();
            }
            //
            var result = LoadData(softVersion);
            foreach(var node in result.TreeNodes)
            {
                UpdownFile(node);
            }
            if (tasks.Count > 0)
            {
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
            }
        }
      
        /// <summary>
        /// 检测版本
        /// </summary>
        public void Check()
        {
            SoftVersion ver = null;
            var lastVer= GetLatest();
            if(lastVer==null||lastVer.Count==0)
            {
                //没有任何版本信息则移除
                return;
            }
            else
            {
                //本地已经是最新版本就不检测了
                if(CurVersion==lastVer[0].Version&&MaxVsersionID==lastVer[0].VersionID)
                {
                    return;
                }
            }
            if (Version == "latest")
            {
                var r = lastVer;
                if (r != null && r.Count > 0)
                {
                    ver = r[0];
                    if(ver.Version==CurVersion&&ver.VersionID==MaxVsersionID)
                    {
                        //已经更新过了
                        return;
                    }
                    if(ver.Clear)
                    {
                        Clear();
                    }
                }
            }
            else
            {
                var r = QueryVersion();
                if (r != null && r.Count > 0)
                {
                    ver = r.FindLast(x => x.Version== Version);
                    var max = r.FindAll(x => x.Version.CompareTo(Version)>0&&x.Clear);
                    if(max!=null&&max.Count>0)
                    {
                        CurVersion = "";
                        Clear();
                    }
                }
            }
            if(ver!=null)
            {
                //比较,版本不同或者更新了文件，则比较
                if(ver.Version!=CurVersion||ver.VersionID!=MaxVsersionID)
                {
                    // 
                    Updown(ver);
                }
                LastVersion = ver;
            }
            else
            {

                //没有找到该版本了，则清空全部下载最新版本

                var r = lastVer;
                if (r != null && r.Count > 0)
                {
                    ver = r[0];
                }
                if(CurVersion==ver.Version&&ver.VersionID==MaxVsersionID)
                {
                    //已经是最新的了
                    return;
                }
                Clear();
                Updown(ver);
                LastVersion = ver;

            }
            //
            ILargerStore store = StoreFactory.GetStore();
            if(store!=null)
            {
                while(!store.GetDownloadComplete())
                {
                    Thread.Sleep(1000);
                }
            }
        }

    }
}
