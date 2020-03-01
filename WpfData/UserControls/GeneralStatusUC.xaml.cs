using System;
using System.Windows.Controls;
using System.Windows.Media;

using WpfData.DataStructures;

namespace WpfData.UserControls
{
    /// <summary>
    /// Logique d'interaction pour GeneralStatusUC.xaml
    /// </summary>
    public partial class GeneralStatusUC : UserControl
    {
        public GeneralStatusUC ( )
        {
            InitializeComponent();
            DataContext = this;
        }

        public void UpdateTextsData (NetworkMeasure data)
        {
            tbMainDisplay.Text = $"{data.TotalMonth.ToString(3)} / {data.TrafficMaxLimit}";

            double percents = Math.Round(data.TotalMonth / data.TrafficMaxLimit * 100, 4);
            tbPercents.Text = $"{percents} %";
            progressBar.Value = percents;

            if ( data.DownloadRate > Octet.FromMega(1) )
            {
                tbNetUse.Text = "Un téléchargement est certainement en cours !";
            }
            else
            {
                tbNetUse.Text = "Utilisation normale de la bande passante.";
            }
        }

        public void SetNetworkStatus (string msg, System.Windows.Media.Color? color = null)
        {
            tbNetStatus.Text = msg;
            if ( color is null )
            {
                tbNetStatus.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                tbNetStatus.Foreground = new SolidColorBrush(color.Value);
            }
        }
    }
}
