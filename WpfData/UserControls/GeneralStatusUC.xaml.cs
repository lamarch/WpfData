using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfData
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

        public void UpdateTextsData(NetworkDataUsage data)
        {
            tbMainDisplay.Text = $"{data.GetTotal().ToString(3)} / {data.TrafficMaxLimit}";

            double percents = Math.Round(data.GetTotal() / data.TrafficMaxLimit * 100, 4);
            tbPercents.Text = $"{percents} %";
            progressBar.Value = percents;
            
            if ( data.CurrentDownloadRate > Octet.FromMega(1) )
            {
                tbNetUse.Text = "Un téléchargement est certainement en cours !";
            }
            else
            {
                tbNetUse.Text = "Utilisation normale de la bande passante.";
            }
        }

        public void SetNetworkStatus(string msg, System.Windows.Media.Color? color = null)
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
