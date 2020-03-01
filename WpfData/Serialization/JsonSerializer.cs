using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace WpfData.Serialization
{
    internal class JsonSerializer<T> : ISerializer<T>
    {
        private string path;
        private T obj;

        public JsonSerializer(string path)
        {
            this.path = path;
        }


        public void Load ( )
        {
            string json = File.ReadAllText(path);
            this.obj = JsonSerializer.Deserialize<T>(json);
        }

        public void Save ( )
        {
            string json = JsonSerializer.Serialize<T>(obj);
            File.WriteAllText(json, path);
        }

        public void Set (T obj) => this.obj = obj;
        public T Get ( ) => obj;

    }
}
