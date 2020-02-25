using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfData
{
    class Logger
    {

        const string folderName = "logs";
        string fileName;

        public string filePath;
        
        public Logger ( )
        {
            fileName = "LOG (v " + AppDataFolder.GetVersion() + ") " + DateTime.Now.ToShortDateString().Replace("/", "-") + "_" + DateTime.Now.ToShortTimeString().Replace(':', '-') + ".txt";
            filePath = AppDataFolder.GetPath(Path.Combine(folderName, fileName));

            AppDataFolder.Views.Add(new Util.FolderView(folderName, null));
        }

        public void Log(string msg)
        {
            AppDataFolder.AccessFolder();
            File.AppendAllText(filePath, DateTime.Now.ToLongTimeString() + " :\t" + msg + "\n\n");
        }

        public void Test (LogType t, LogLevel l, string name, string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = -1)
        {
            AppDataFolder.AccessFolder();

            string final = $"{l} / {t} [{caller} at {line}], {name.ToUpperInvariant()} : {message}";
        }

        public enum LogType
        {
            Event,
            Exception,
            Value
        }

        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Critic
        }
    }
}
