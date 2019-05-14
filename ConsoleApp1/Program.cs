using SoftUpdate;
using System;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            VersionFile file = new VersionFile();
            file.Check();
            Console.WriteLine("完成!");
            if(file.FileRename)
            {
                Process.Start("FileUpdate.exe");
               
            }
        }
    }
}
