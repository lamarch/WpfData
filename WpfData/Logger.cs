using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfData
{
    class Logger
    {
        string fileName;
        public string filePath;
        
        public Logger ( )
        {
            fileName = "LOG " + DateTime.Now.ToShortDateString().Replace("/", "-") + "_" + DateTime.Now.ToShortTimeString() + ".txt";
            filePath = AppDataFolder.GetPath(fileName);
        }

        public void Log(string msg)
        {
            AppDataFolder.AccessFolder();
            File.AppendAllText(filePath, DateTime.Now.ToShortTimeString() + "\t:\t" + msg + "\n\n");
        }
    }
}
