using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace WpfData.Util
{
    internal class DataSerializer<T>
    {
        private string path;
        private T obj = default;

        public DataSerializer (string path)
        {
            this.path = path;
        }

        public T Get ( ) => this.obj;

        public void Set (T obj) => this.obj = obj;

        public async Task Load ( )
        {

            BinaryFormatter formatter = new BinaryFormatter();
            using ( Stream s = new FileStream(this.path, FileMode.Open, FileAccess.Read, FileShare.None) )
            {
                this.obj = await new Task<T>(( ) => (T)formatter.Deserialize(s));
            }
        }

        public async Task Save ( )
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using ( Stream s = new FileStream(this.path, FileMode.Create, FileAccess.Write, FileShare.None) )
            {
                await new Task(( ) => formatter.Serialize(s, this.obj));
            }
        }
    }
}
