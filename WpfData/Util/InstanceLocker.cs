using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfData.Util
{
    internal class InstanceLocker
    {
        private static readonly string lockFile = AppDataFolder.GetPath(".lock");
        FileStream s;
        public InstanceLocker ()
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
