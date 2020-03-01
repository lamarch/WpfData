using System.Collections.Generic;

namespace WpfData.Requests
{
    internal class RequestParserConfig
    {
        public string netAdress;
        public List<RequestParserFile> files;

        public RequestParserConfig (string netAdress, List<RequestParserFile> files)
        {
            this.netAdress = netAdress;
            this.files = files;
        }
    }

    internal class RequestParserFile
    {
        public Dictionary<string, NetworkMeasureProperty> equivs;
        public string name;

        public RequestParserFile (string _name, params (string requestName, NetworkMeasureProperty propEquiv)[] _equivs)
        {
            this.name = _name;
            this.equivs = new Dictionary<string, NetworkMeasureProperty>();
            foreach ( var item in _equivs )
            {
                equivs.Add(item.requestName, item.propEquiv);
            }
        }
    }

    public enum NetworkMeasureProperty
    {
        TotalDownload,
        TotalUpload,
        DownloadRate,
        UploadRate,
        TrafficMaxLimit,
        StartDay
    }
}
