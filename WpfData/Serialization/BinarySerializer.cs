using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace WpfData.Serialization
{
    internal class BinarySerializer<T> : ISerializer<T>
    {
        private string path;
        private T obj = default;

        public BinarySerializer (string path)
        {
            this.path = path;
        }

        public T Get ( ) => this.obj;

        public void Set (T obj) => this.obj = obj;

        public void Load ( )
        {

            BinaryFormatter formatter = new BinaryFormatter();
            using ( Stream s = new FileStream(this.path, FileMode.Open, FileAccess.Read, FileShare.None) )
            {
                this.obj = (T)formatter.Deserialize(s);
            }
        }

        public void Save ( )
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using ( Stream s = new FileStream(this.path, FileMode.Create, FileAccess.Write, FileShare.None) )
            {
                formatter.Serialize(s, this.obj);
            }
        }
    }
}
