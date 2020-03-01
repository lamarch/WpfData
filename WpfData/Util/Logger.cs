using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace WpfData.Util
{
    internal class Logger
    {

        private const string folderName = "logs";
        private static readonly string subFolderName = AppDataFolder.GetVersion();

        public readonly string fileName;
        public readonly string filePath;

        public Logger ( )
        {
            fileName = $"LOG (v_{AppDataFolder.GetVersion()}) {DateTime.Now.ToString("G").Replace(':', '-').Replace("/", "-")}.txt";

            filePath = AppDataFolder.GetPath(Path.Combine(folderName, subFolderName, fileName));

            AppDataFolder.Views.Add(new Util.FolderView(folderName, new Util.FolderView(subFolderName, null)));
        }

        public void Log (LogType t, LogLevel l, string name, object message = null, [CallerMemberName] string caller = "Unknown", [CallerLineNumber] int line = -1)
        {
            AppDataFolder.AccessFolder();



            string finalMessage = "None";
            finalMessage = message?.ToString();

            string final = $"{DateTime.Now.ToString()}\t{l} / {t} [{caller} at {line}], {name.ToUpperInvariant()} : {finalMessage}\n\n";

            File.AppendAllText(filePath, final);
        }

        public void LogException (string name, Exception ex, [CallerMemberName] string caller = "Unknown", [CallerLineNumber] int line = -1) => Log(LogType.Exception, LogLevel.Error, name, ex, caller, line);

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
