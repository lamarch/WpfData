using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.XPath;
using LiveCharts;

namespace WpfData
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        const string path = @"\\PM54\Users\Xavier\source\repos\WpfData\WpfData\bin\WpfData_Release\WpfData.exe";


        private readonly Regex regexInputLong = new Regex(@"[\d]+");
        private readonly SolidColorBrush colorNormalWhite = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush colorWarning = new SolidColorBrush(Colors.Orange);
        private readonly SolidColorBrush colorHighWarning = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush colorNormalBlack = new SolidColorBrush(Colors.Black);
        private readonly SolidColorBrush colorGood = new SolidColorBrush(Colors.Green);


        private readonly Dictionary<string, long> units = new Dictionary<string, long>() { 
            { "MO", 1024*1024 }, 
            { "GO", 1024*1024*1024 } };


        StackFrame callStack = new StackFrame(1, true);
        DispatcherTimer dispatcher;
        DataRequester requester;
        NotifyIcon icon;
        XmlParser<NetworkDataUsage> xmlParser;
        Logger logger;
        System.Windows.Forms.MenuItem menuItem_icon_close;

        ChartValues<NetworkDataUsage> timeUsage = new ChartValues<NetworkDataUsage>();


        public MainWindow ( )
        {
            DataContext = this;
            
            InitializeComponent();

            //config the log system
            logger = new Logger();
            logger.Log("appStart EVENT");
            //config the xml parser
            xmlParser = new XmlParser<NetworkDataUsage>("CurrentMonthDownload", "CurrentMonthUpload", "CurrentDownloadRate", "CurrentUploadRate", "trafficmaxlimit", "StartDay");

            //config the "httpclient"
            requester = new DataRequester("http://192.168.1.1/api/monitoring/traffic-statistics", "http://192.168.1.1/api/monitoring/start_date", "http://192.168.1.1/api/monitoring/month_statistics");
            requester.GetReady();
            
            //config the timer
            dispatcher = new DispatcherTimer();
            dispatcher.Interval = TimeSpan.FromSeconds(1.5f);
            dispatcher.Tick += this.timerTick;
            dispatcher.Start();

            //config the taskbar icon menu item
            menuItem_icon_close = new System.Windows.Forms.MenuItem("Fermer l'application", new EventHandler((a, b) => System.Windows.Application.Current.Shutdown()));

            //config the task bar icon
            icon = new NotifyIcon();
            icon.Visible = true;
            icon.Icon = Properties.Resources.icon;
            icon.Click += this.Icon_Click;
            icon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] { menuItem_icon_close });

            //config the shutdown
            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            System.Windows.Application.Current.Exit += Application_ApplicationExit;

        }




        private async void timerTick (object sender, EventArgs e)
        {
            NetworkDataUsage data = new NetworkDataUsage();
            Cursor = System.Windows.Input.Cursors.Wait;
            logger.Log("timerTick EVENT");
            if ( requester.IsReady() )
            {
                List<string> responses = requester.Get();

                try
                {
                    xmlParser.Parse(data, responses.ToArray());
                }catch(Exception ex)
                {
                    logger.Log("xmlParser.Parse EXCEPTION (" + ex.Message + ")");
                    SetNetworkStatus("problème formatage/réseau (0x300)", Colors.Red);
                    return;
                }

                if(timeUsage.Count > 300 )
                {
                    timeUsage.RemoveAt(0);
                }

                timeUsage.Add(data);

                SetWindowTextsData(data);
                SetNetworkStatus("OK");

            }
            else
            {
                logger.Log("requester.IsReady FALSE_VALUE");
                SetNetworkStatus("difficultées réseau rencontrées (0x101)", Colors.Orange);
                await Task.Delay(100);
                timerTick(sender, e);
                return;
            }

            try
            {
                requester.GetReady();
            }
            catch(Exception ex)
            {
                logger.Log("requester.GetReady EXCEPTION (" + ex.Message + ")");
                SetNetworkStatus("problème connexion réseau (0x100)", Colors.Red);
            }

            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private int GetDaysBetween (DateTime start, DateTime stop)
        {
            return (int)(stop - start).TotalDays;
        }

        private void SetWindowTextsData(NetworkDataUsage data)
        {

            double percent = Math.Round(data.GetTotal() / data.TrafficMaxLimit * 100, 2);


            lblData.Content = $"{data.GetTotal().ToString(false)} / {data.TrafficMaxLimit.ToString()}";
            lblDataDown.Content = data.CurrentMonthDownload.ToString(3);
            lblDataUp.Content = data.CurrentMonthUpload.ToString(3);

            lblDataDownRate.Content = data.CurrentDownloadRate.ToString(3);
            lblDataUpRate.Content = data.CurrentUploadRate.ToString(3);

            prgData.Value = percent;
            lblDataPercents.Content = percent + "%";


            if ( data.CurrentDownloadRate > Octet.FromMega(5) )
            {
                lblDataDownRate.Foreground = colorHighWarning;
                lblNetUsage.Content = "Un téléchargement est très certainement en cours !";
            }
            else if ( data.CurrentDownloadRate > Octet.FromMega(1) )
            {
                lblDataDownRate.Foreground = colorWarning;
                lblNetUsage.Content = "Un téléchargement est très certainement en cours !";
            }
            else
            {
                lblDataDownRate.Foreground = colorNormalBlack;
                lblNetUsage.Content = "Utilisation normal du réseau.";
            }


            if ( data.CurrentUploadRate > Octet.FromMega(2) )
            {
                lblDataUpRate.Foreground = colorHighWarning;
            }
            else if ( data.CurrentUploadRate > Octet.FromKilo(500) )
            {
                lblDataUpRate.Foreground = colorWarning;
            }
            else
            {
                lblDataUpRate.Foreground = colorNormalBlack;
            }

            SetNotifyIconTextData($"{data.GetTotal().ToString(false)} / {data.TrafficMaxLimit.ToString()} \n({percent}%)");

            if ( percent >= 95 )
            {
                this.Background = colorHighWarning;
            }
            else if ( percent >= 75 )
            {
                this.Background = colorWarning;
            }
            else
            {
                this.Background = colorGood;
            }

            int daysInMonth = GetDaysBetween(DateTime.Now, DateTime.Now.AddMonths(1));

            int startDay = data.StartDay;
            DateTime now = DateTime.Now;
            int remainingDays;

            if(now.Day < startDay )
            {
                remainingDays = startDay - now.Day;
            }
            else
            {
                remainingDays = DateTime.DaysInMonth(now.Year, now.Month) - now.Day + startDay;
            }

            //At the start, the data per day we have
            Octet dataPerDay = data.TrafficMaxLimit / daysInMonth;

            //Now, the data remaining per remaining day
            Octet dataRemainPerDay = (data.TrafficMaxLimit - data.GetTotal()) / remainingDays;


            lblDataPerDayStart.Content = dataPerDay.ToString();

            lblDataPerDayRemain.Content = dataRemainPerDay.ToString();

            if ( dataPerDay > dataRemainPerDay )
            {
                lblDataPerDayRemain.Foreground = colorHighWarning;
            }
            else
            {
                lblDataPerDayRemain.Foreground = colorGood;
            }


        }

        private void SetNetworkStatus(string msg, System.Windows.Media.Color? color = null)
        {
            lblNetworkStatus.Content = msg;
            if(color is null )
            {
                lblNetworkStatus.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                lblNetworkStatus.Foreground = new SolidColorBrush(color.Value);
            }
        }

        private void SetNotifyIconTextData(string msg)
        {
            icon.Text = "WpfData\n" + msg;
        }

        private void VerifyUpdates ( )
        {
            if ( File.Exists(path) )
            {
                FileInfo f = new FileInfo(path);
            }
        }



        private void Icon_Click (object sender, EventArgs e) => Show();

        //Application exit
        private void Application_ApplicationExit (object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            logger.Log("ApplicationExit EVENT");
        }

        private void inbDataPMonth_PreviewTextInput (object sender, TextCompositionEventArgs e)
        {
            e.Handled = !regexInputLong.IsMatch(e.Text);
        }

        private void ckbOver_Click (object sender, RoutedEventArgs e)
        {
            if ( ckbOver.IsChecked.HasValue)
            {
                this.Topmost = ckbOver.IsChecked.Value;
            }
        }


        //Close window
        protected override void OnClosing (CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void btShowGraphs_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                ChartsWindow chartsWindow = new ChartsWindow(timeUsage);
                chartsWindow.Topmost = this.Topmost;
                chartsWindow.Show();
            }catch(Exception ex )
            {
                logger.Log("graphs EXCEPTION (" + ex + ")");
                System.Windows.MessageBox.Show("Désolé, une erreur est survenue lors de l'affichage des graphiques. Merci d'en avertir le développeur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btShowLogs_Click (object sender, RoutedEventArgs e)
        {
            if ( File.Exists(logger.filePath) )
            {
                Process.Start("explorer.exe", "/select,\"" + logger.filePath + "\"");
            }
            else
            {
                AppDataFolder.AccessFolder();
                Process.Start("explorer.exe", "\"" + AppDataFolder.FolderPath + "\"");
            }
        }
    }
}
