using System;
using System.IO;
namespace FileUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Rename();
        }


        static void Rename()
        {
            string[] files = Directory.GetFileSystemEntries(AppDomain.CurrentDomain.BaseDirectory, "*.tmp");
            if(files.Length>0)
            {
                foreach(string file in files)
                {
                    FileInfo info = new FileInfo(file);
                    string newFile = info.FullName.Remove(info.FullName.Length - 4);
                    if(info.Exists)
                    {
                        if(File.Exists(newFile))
                        {
                            File.Delete(newFile);
                        }
                        info.MoveTo(newFile);
                    }
                }
            }
        }
    }
}
