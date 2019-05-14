#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ILargerFileStore
* 项目描述 ：
* 类 名 称 ：LargeStore
* 类 描 述 ：
* 命名空间 ：ILargerFileStore
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

namespace ILargerFileStore
{

    /* ============================================================================== 
* 功能描述：LargeStore 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
   public class LargeStore
    {
        public int ID { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }

        public string LocalName { get; set; }

        public string LocalFile { get; set; }
    }
}
