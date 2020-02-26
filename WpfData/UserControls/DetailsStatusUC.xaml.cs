using System;
using System.Linq;
using System.Windows.Controls;

using LiveCharts;

namespace WpfData
{
    /// <summary>
    /// Logique d'interaction pour detailsStatusUC.xaml
    /// </summary>
    public partial class DetailsStatusUC : UserControl
    {

        public DetailsStatusUC ()
        {
            InitializeComponent();
            DataContext = this;

        }

        public void UpdateTextsData(NetworkDataUsage newData, NetworkDataUsage oldData)
        {
            //text

            tbDownTotal.Text = newData.CurrentMonthDownload;
            tbUpTotal.Text = newData.CurrentMonthUpload;

            tbDownTraffic.Text = newData.CurrentDownloadRate;
            tbUpTraffic.Text = newData.CurrentUploadRate;

            tbDownEv.Text = newData.CurrentMonthDownload - oldData.CurrentMonthDownload;
            tbUpEv.Text = newData.CurrentMonthUpload - oldData.CurrentMonthUpload;

            tbHeaderEv.Text = "Evolution depuis " + oldData.DateTime.ToString("HH:mm");


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
            Octet newDataRemainPerDay = (newData.TrafficMaxLimit - newData.GetTotal()) / remainingDays;


            this.lblDataPerDayStart.Content = newDataPerDay.ToString();

            this.lblDataPerDayRemain.Content = newDataRemainPerDay.ToString();

            if ( newDataPerDay > newDataRemainPerDay )
            {
                this.lblDataPerDayRemain.Foreground = MainWindow.colorHighWarning;
            }
            else
            {
                this.lblDataPerDayRemain.Foreground = MainWindow.colorGood;
            }



            //colorization

            if ( newData.CurrentDownloadRate > Octet.FromMega(5) )
            {
                this.tbDownTraffic.Foreground = MainWindow.colorHighWarning;
            }
            else if ( newData.CurrentDownloadRate > Octet.FromMega(1) )
            {
                this.tbDownTraffic.Foreground = MainWindow.colorWarning;
            }
            else
            {
                this.tbDownTraffic.Foreground = MainWindow.colorNormalBlack;
            }

            if ( newData.CurrentUploadRate > Octet.FromMega(2) )
            {
                this.tbUpTraffic.Foreground = MainWindow.colorHighWarning;
            }
            else if ( newData.CurrentUploadRate > Octet.FromKilo(500) )
            {
                this.tbUpTraffic.Foreground = MainWindow.colorWarning;
            }
            else
            {
                this.tbUpTraffic.Foreground = MainWindow.colorNormalBlack;
            }

            int GetDaysBetween (DateTime start, DateTime stop) => (int)(stop - start).TotalDays;
        }

    }
}
