using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;

namespace WpfData.Util
{
    internal static class AppDataFolder
    {
        public static string FolderName = "WpfData";
        public static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FolderName);

        public static List<FolderView> Views = new List<FolderView>();

        public static void AccessFolder ( )
        {
            if ( !Directory.Exists(FolderPath) )
            {
                Directory.CreateDirectory(FolderPath);
            }

            foreach ( FolderView fw in Views )
            {
                if ( fw != null )
                {
                    fw.PrepareAccess(FolderPath);
                }
            }

        }

        public static string GetPath (string internalPath) => Path.Combine(FolderPath, internalPath);

        public static string GetVersion ( )
        {
            string version = null;
            try
            {
                //// get deployment version
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch ( InvalidDeploymentException )
            {
                //// you cannot read publish version when app isn't installed 
                //// (e.g. during debug)
                version = "0";
            }

            return version;
        }
    }
}
