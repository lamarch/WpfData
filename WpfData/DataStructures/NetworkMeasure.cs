using System;
using WpfData.Requests;

namespace WpfData.DataStructures
{
    [Serializable]
    public class NetworkMeasure
    {
        public readonly Octet TotalDownload = new Octet();
        public readonly Octet TotalUpload = new Octet();
        public readonly Octet DownloadRate = new Octet();
        public readonly Octet UploadRate = new Octet();

        public readonly Octet TrafficMaxLimit = new Octet();
        public readonly int StartDay = 0;

        public readonly DateTime DateTime;

        public NetworkMeasure ( )
        {
            DateTime = DateTime.Now;
        }

        public NetworkMeasure (Octet totalDownload, Octet totalUpload, Octet downloadRate, Octet uploadRate, Octet trafficMaxLimit, int startDay, DateTime dateTime)
        {
            this.TotalDownload = totalDownload;
            this.TotalUpload = totalUpload;
            this.DownloadRate = downloadRate;
            this.UploadRate = uploadRate;
            this.TrafficMaxLimit = trafficMaxLimit;
            this.StartDay = startDay;
            this.DateTime = dateTime;
        }

        public Octet TotalMonth => TotalDownload + TotalUpload;
        public Octet Rate => UploadRate + DownloadRate;

        public void SetProperty (NetworkMeasureProperty prop, string val)
        {
            long dbVal = long.Parse(val);

            switch ( prop )
            {
                case NetworkMeasureProperty.DownloadRate:
                case NetworkMeasureProperty.TotalDownload:
                case NetworkMeasureProperty.TotalUpload:
                case NetworkMeasureProperty.UploadRate:
                case NetworkMeasureProperty.TrafficMaxLimit:
                    SetProperty(prop, Octet.FromOctet(dbVal));
                    break;

                case NetworkMeasureProperty.StartDay:
                    SetProperty(prop, Convert.ToInt32(dbVal));
                    break;
            }
        }

        private void SetProperty<T> (NetworkMeasureProperty name, T val) => GetType().GetField(name.ToString()).SetValue(this, val);
    }
}
