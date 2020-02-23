using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfData
{
    static class AppDataFolder
    {
        public static string FolderName = "WpfData";
        public static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FolderName);
        public static void AccessFolder ( )
        {
            if ( !Directory.Exists(FolderPath) )
                Directory.CreateDirectory(FolderPath);
        }

        public static string GetPath(string internalPath)
        {
            return Path.Combine(FolderPath, internalPath);
        }
    }
}
