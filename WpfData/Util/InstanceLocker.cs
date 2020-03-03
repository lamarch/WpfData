using System.IO;

namespace WpfData.Util
{
    internal class InstanceLocker
    {
        private static readonly string lockFile = AppDataFolder.GetPath(".lock");
        FileStream s;
        public InstanceLocker ( )
        {

        }

        public void Lock ( )
        {
            if ( !File.Exists(lockFile) )
            {
                s = new FileStream(lockFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            }
        }

        public void Unlock ( )
        {
            if ( s != null )
                s.Close();
            if ( File.Exists(lockFile) )
            {
                File.Delete(lockFile);
            }
        }

        public bool Check ( )
        {
            AppDataFolder.AccessFolder();
            return !File.Exists(lockFile);
        }
    }
}
