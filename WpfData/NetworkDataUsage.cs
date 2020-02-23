using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfData
{
    public class NetworkDataUsage : IXmlParserObject
    {
        public readonly Octet CurrentMonthDownload = new Octet();
        public readonly Octet CurrentMonthUpload = new Octet();
        public readonly Octet CurrentDownloadRate = new Octet();
        public readonly Octet CurrentUploadRate = new Octet();

        public readonly Octet TrafficMaxLimit = new Octet();
        public readonly int StartDay = 0;

        public readonly DateTime DateTime;

        public NetworkDataUsage ( )
        {
            DateTime = DateTime.Now;
        }

        public Octet GetTotal ( ) => CurrentMonthDownload + CurrentMonthUpload;

        public void SetProperty (string name, string val)
        {
            dynamic value = long.Parse(val);
            if ( name != "StartDay" )
                value = Octet.FromOctet(value);
            else
                value = (int)value;
            FieldInfo finfo = GetType().GetField(name, System.Reflection.BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            finfo.SetValue(this, value);
        }
    }
}
