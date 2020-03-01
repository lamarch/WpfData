using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfData.Serialization
{
    interface ISerializer<T>
    {
        T Get ( );
        void Set (T obj);
        void Load ( );
        void Save ( );
    }
}
