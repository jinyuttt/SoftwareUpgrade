#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：SoftUpdate
* 项目描述 ：
* 类 名 称 ：VsersionInfo
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

using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUpdate
{

    /* ============================================================================== 
* 功能描述：VsersionInfo 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
  public  class VersionInfo
    {
        public VersionInfo()
        {
            Version = "latest";
            LocalVersion = "";
        }
        /// <summary>
        /// 控制检测版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 本地版本
        /// </summary>
        public string LocalVersion { get; set; }

        /// <summary>
        /// 本地版本ID
        /// </summary>
        public int LocalVersionID { get; set; }

    }
}
