using System;
using System.Collections.Generic;

namespace WpfData.Requests
{
    internal class RequestParserFile
    {
        public Dictionary<string, NetworkMeasureProperty> equivs;
        public string path;

        public RequestParserFile (string path, params (string requestName, NetworkMeasureProperty propEquiv)[] _equivs)
        {
            this.path = path;
            this.equivs = new Dictionary<string, NetworkMeasureProperty>();
            foreach ( var item in _equivs )
            {
                equivs.Add(item.requestName, item.propEquiv);
            }
        }
    }

    public enum NetworkMeasureProperty
    {
        CurrentDownload,
        CurrentUpload,
        DownloadRate,
        UploadRate,
        TrafficMaxLimit,
        StartDay
    }
}
