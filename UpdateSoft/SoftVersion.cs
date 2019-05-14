#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UpdateSoft
* 项目描述 ：
* 类 名 称 ：SoftVersion
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


namespace UpdateSoft
{

    /* ============================================================================== 
* 功能描述：SoftVersion 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class SoftVersion
    {
        public string Version { get; set; }
        public int ID { get; set; }

        [EntityMappingDB.DataField("VerTime")]
        public string CreateTime { get; set; }

        [EntityMappingDB.DataField("versionid")]
        public int MaxVerID { get; set; }
    }
}
