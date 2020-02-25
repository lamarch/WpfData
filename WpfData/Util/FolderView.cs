using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents.DocumentStructures;

namespace WpfData.Util
{
    internal class FolderView
    {
        public List<FolderView> ChildsView;
        public string Name;

        public FolderView (string name, params FolderView[] views)
        {
            this.Name = name;

            if(views == null )
            {
                views = new FolderView[0];
            }
            this.ChildsView = views.ToList();
        }

        public FolderView(string name) : this(name, null) { }

        public void PrepareAccess (string parentPath)
        {
            string actualPath = Combine(parentPath, Name);

            if ( !Exist(actualPath) )
            {
                Create(actualPath);
            }

            if(ChildsView != null )
            {
                foreach ( var view in this.ChildsView )
                {
                    view.PrepareAccess(actualPath);
                }
            }
        }

        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public bool Exist(string folder)
        {
            return Directory.Exists(folder);
        }

        public DirectoryInfo Create(string path)
        {
            return Directory.CreateDirectory(path);
        }
    }
}
