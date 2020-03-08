using System;
using System.Windows.Controls;

using WpfData.DataStructures;

namespace WpfData.UserControls
{
    /// <summary>
    /// Logique d'interaction pour detailsStatusUC.xaml
    /// </summary>
    public partial class DetailsStatusUC : UserControl
    {

        public DetailsStatusUC ( )
        {
            InitializeComponent();
            DataContext = this;

        }

        public void UpdateTextsData (NetworkMeasure newData, NetworkMeasure oldData)
        {
            //text

            tbDownTotal.Text = newData.CurrentDownload;
            tbUpTotal.Text = newData.CurrentUpload;

            tbDownTraffic.Text = newData.DownloadRate;
            tbUpTraffic.Text = newData.UploadRate;

            tbDownEv.Text = newData.CurrentDownload - oldData.CurrentDownload;
            tbUpEv.Text = newData.CurrentUpload - oldData.CurrentUpload;

            tbHeaderEv.Text = "Evolution depuis " + oldData.DateTime.ToString("T");

            //start day
            lblStartDay.Content = newData.StartDay;


            //Days

            int daysInMonth = GetDaysBetween(DateTime.Now, DateTime.Now.AddMonths(1));

            int startDay = newData.StartDay;
            DateTime now = DateTime.Now;
            int remainingDays;

            if ( now.Day < startDay )
            {
                remainingDays = startDay - now.Day;
            }
            else
            {
                remainingDays = DateTime.DaysInMonth(now.Year, now.Month) - now.Day + startDay;
            }

            //At the start, the newData per day we have
            Octet newDataPerDay = newData.TrafficMaxLimit / daysInMonth;

            //Now, the newData remaining per remaining day
            Octet newDataRemainPerDay = (newData.TrafficMaxLimit - newData.TotalMonth) / remainingDays;


            this.lblDataPerDayStart.Content = newDataPerDay.ToString();

            this.lblDataPerDayRemain.Content = newDataRemainPerDay.ToString();

            if ( newDataPerDay > newDataRemainPerDay )
            {
                this.lblDataPerDayRemain.Foreground = App.colorHighWarning;
            }
            else
            {
                this.lblDataPerDayRemain.Foreground = App.colorGood;
            }



            //colorization

            if ( newData.DownloadRate > Octet.FromMega(5) )
            {
                this.tbDownTraffic.Foreground = App.colorHighWarning;
            }
            else if ( newData.DownloadRate > Octet.FromMega(1) )
            {
                this.tbDownTraffic.Foreground = App.colorWarning;
            }
            else
            {
                this.tbDownTraffic.Foreground = App.colorNormalBlack;
            }

            if ( newData.UploadRate > Octet.FromMega(2) )
            {
                this.tbUpTraffic.Foreground = App.colorHighWarning;
            }
            else if ( newData.UploadRate > Octet.FromKilo(500) )
            {
                this.tbUpTraffic.Foreground = App.colorWarning;
            }
            else
            {
                this.tbUpTraffic.Foreground = App.colorNormalBlack;
            }

            int GetDaysBetween (DateTime start, DateTime stop) => (int)(stop - start).TotalDays;
        }

    }
}
