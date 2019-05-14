#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UpdateSoft
* 项目描述 ：
* 类 名 称 ：ILargerStore
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


namespace ILargerFileStore
{

    /* ============================================================================== 
* 功能描述：ILargerStore 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public  interface ILargerStore
    {
        

       /// <summary>
       /// 准备提交的文件
       /// </summary>
       /// <param name="id">存储表中唯一标识</param>
       /// <param name="localFile">本地文件</param>
       /// <param name="remoteName">远端名称</param>
       /// <returns>远端路径</returns>
        string CommitFile(int id,string localFile,out string remoteName);

        /// <summary>
        /// 下载远端文件
        /// </summary>
        /// <param name="remoteFile">远端地址</param>
        /// <param name="localFile">本地文件</param>
        void DownloadFile(LargeStore storeinfo);

        /// <summary>
        /// 提交文件是否完成；避免异步实现
        /// </summary>
        /// <returns></returns>
        bool GetCommitComplete();

        /// <summary>
        /// 加载文件是否完成；避免异步实现
        /// </summary>
        /// <returns></returns>
        bool GetDownloadComplete();
    }
}
