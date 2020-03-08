using System;
using System.Collections.Generic;
using WpfData.Requests;

namespace WpfData.DataStructures
{
    internal class Configuration
    {
        public Uri netAdress;
        public List<RequestParserFile> files;

        public Configuration ( )
        {
            netAdress = new Uri("http://192.168.1.1");
        }

        public Configuration (Uri netAdress, List<RequestParserFile> files)
        {
            this.netAdress = netAdress;
            this.files = files;
        }
    }
}
