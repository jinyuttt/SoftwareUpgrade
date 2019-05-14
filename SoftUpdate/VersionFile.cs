#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：SoftUpdate
* 项目描述 ：
* 类 名 称 ：VersionFile
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

using Newtonsoft.Json;
using System;
using System.IO;

namespace SoftUpdate
{

    /* ============================================================================== 
* 功能描述：VersionFile 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class VersionFile
    {
        /// <summary>
        /// 配置文件完整路径
        /// </summary>
        public string VersionFilePath { get; set; }

        /// <summary>
        /// 软件根目录
        /// </summary>
        public string SoftPath { get; set; }

        /// <summary>
        /// 是否需要重置文件名称
        /// </summary>
        /// 
        public bool FileRename { get; set; }


        public VersionFile()
        {
            VersionFilePath = "Version.json";
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <returns></returns>
        private VersionInfo ReadLocal()
        {
            string json = "";
            if(File.Exists(VersionFilePath))
            {
                using (StreamReader rd = new StreamReader(VersionFilePath))
                {
                    json = rd.ReadToEnd();
                }
            }
             return JsonConvert.DeserializeObject<VersionInfo>(json);
        }

       /// <summary>
       /// 保持配置
       /// </summary>
       /// <param name="info"></param>
        private void SaveFile(VersionInfo info)
        {
            string json = JsonConvert.SerializeObject(info);
            if (!File.Exists(VersionFilePath))
            {
                using (StreamWriter sw = new StreamWriter(VersionFilePath))
                {
                    sw.WriteLine(json);
                }
            }
        }

        /// <summary>
        /// 验证文件更新
        /// </summary>
        public void Check()
        {
            var info = ReadLocal();
            if (string.IsNullOrEmpty(SoftPath))
            {
                SoftPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            if(info==null)
            {
                info = new VersionInfo();
            }
            SoftList soft = new SoftList()
            {
                CureentPath = SoftPath,
                CurVersion = info.LocalVersion,
                Version = info.Version,
                MaxVsersionID = info.LocalVersionID
            };
            soft.Check();
            
            FileRename = soft.FileRename;
            if(soft.LastVersion!=null)
            {
                VersionInfo version = new VersionInfo()
                {
                    LocalVersion = soft.LastVersion.Version,
                    LocalVersionID = soft.LastVersion.VersionID,
                    Version = info.Version
                };
                SaveFile(version);
            }
           
          }
    }
}
