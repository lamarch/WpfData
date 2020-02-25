using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

using LiveCharts;

using WpfData.Util;

namespace WpfData
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string updatePath = @"\\PM54\Users\Xavier\source\repos\WpfData\WpfData\publish\setup.exe";


        private readonly Regex regexInputLong = new Regex(@"[\d]+");
        private readonly SolidColorBrush colorNormalWhite = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush colorWarning = new SolidColorBrush(Colors.Orange);
        private readonly SolidColorBrush colorHighWarning = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush colorNormalBlack = new SolidColorBrush(Colors.Black);
        private readonly SolidColorBrush colorGood = new SolidColorBrush(Colors.Green);
        public readonly bool IsDebugMode = false;

        private readonly Dictionary<string, long> units = new Dictionary<string, long>() {
            { "MO", 1024*1024 },
            { "GO", 1024*1024*1024 } };
        private StackFrame callStack = new StackFrame(1, true);
        private DispatcherTimer dispatcher;
        private DataRequester requester;
        private NotifyIcon icon;
        private XmlParser<NetworkDataUsage> xmlParser;
        private Logger logger;
        private System.Windows.Forms.MenuItem menuItem_icon_close;
        private ChartValues<NetworkDataUsage> timeUsage = new ChartValues<NetworkDataUsage>();


        public MainWindow ( )
        {

#if DEBUG
            this.IsDebugMode = true;
#endif

            //config the log system
            this.logger = new Logger();
            this.logger.Log("EVENT appStart");


            //config the window
            this.InitializeComponent();
            DataContext = this;
            Title = "WpfData v_" + AppDataFolder.GetVersion();


            //config the data layer (serialization)
            DataLayer.Init(logger);
            DataLayer.LoadData();


            //config the xml parser
            this.xmlParser = new XmlParser<NetworkDataUsage>("CurrentMonthDownload", "CurrentMonthUpload", "CurrentDownloadRate", "CurrentUploadRate", "trafficmaxlimit", "StartDay");


            //config the "httpclient"
            this.requester = new DataRequester("http://192.168.1.1/api/monitoring/traffic-statistics", "http://192.168.1.1/api/monitoring/start_date", "http://192.168.1.1/api/monitoring/month_statistics");
            this.requester.GetReady();


            //config the timer
            this.dispatcher = new DispatcherTimer();
            this.dispatcher.Interval = TimeSpan.FromSeconds(1.5f);
            this.dispatcher.Tick += this.timerTick;
            this.dispatcher.Start();


            //config the taskbar icon menu item
            this.menuItem_icon_close = new System.Windows.Forms.MenuItem("Fermer l'application", new EventHandler((a, b) => System.Windows.Application.Current.Shutdown()));


            //config the task bar icon
            this.icon = new NotifyIcon();
            this.icon.Visible = true;
            this.icon.Icon = Properties.Resources.icon;
            this.icon.Click += this.Icon_Click;
            this.icon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] { this.menuItem_icon_close });


            //config the shutdown
            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            System.Windows.Application.Current.Exit += this.Application_ApplicationExit;



        }

        private bool lastRequestNotReady;

        private void timerTick (object sender, EventArgs e)
        {
            NetworkDataUsage data = new NetworkDataUsage();
            Cursor = System.Windows.Input.Cursors.Wait;


            if ( this.requester.IsReady() )
            {
                //To change the log status
                if ( this.lastRequestNotReady )
                {
                    this.logger.Log("INFO requester.IsReady TRUE_VALUE");
                }


                this.lastRequestNotReady = false;

                List<string> responses = this.requester.Get();


                try
                {
                    this.xmlParser.Parse(data, responses.ToArray());
                }
                catch ( Exception ex )
                {
                    this.logger.Log("EXCEPTION xmlParser.Parse (" + ex.Message + ")");
                    this.generalStatusUC.SetNetworkStatus("problème formatage/réseau (0x300)", Colors.Red);
                    return;
                }

                this.timeUsage.Add(data);

                this.SetWindowTextsData(data);
                this.generalStatusUC.SetNetworkStatus("OK");

            }
            else
            {
                //To avoid to many logs at once
                if ( !lastRequestNotReady )
                {
                    this.logger.Log("INFO requester.IsReady() FALSE_VALUE");
                    this.generalStatusUC.SetNetworkStatus("difficultées réseau rencontrées (0x101)", Colors.Orange);
                }

                this.lastRequestNotReady = true;

                return;
            }

            try
            {
                this.requester.GetReady();
            }
            catch ( Exception ex )
            {
                this.logger.Log("EXCEPTION requester.GetReady (" + ex.Message + ")");
                this.generalStatusUC.SetNetworkStatus("problème connexion réseau (0x100)", Colors.Red);

            }

            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void SetWindowTextsData (NetworkDataUsage data)
        {

            this.generalStatusUC.UpdateTextsData(data);


            this.lblDataDown.Content = data.CurrentMonthDownload.ToString(3);
            this.lblDataUp.Content = data.CurrentMonthUpload.ToString(3);

            this.lblDataDownRate.Content = data.CurrentDownloadRate.ToString(3);
            this.lblDataUpRate.Content = data.CurrentUploadRate.ToString(3);


            if ( data.CurrentDownloadRate > Octet.FromMega(5) )
            {
                this.lblDataDownRate.Foreground = this.colorHighWarning;
                this.generalStatusUC.SetNetworkUse("Un téléchargement est très certainement en cours !");
            }
            else if ( data.CurrentDownloadRate > Octet.FromMega(1) )
            {
                this.lblDataDownRate.Foreground = this.colorWarning;
                this.generalStatusUC.SetNetworkUse("Un téléchargement est très certainement en cours !");
            }
            else
            {
                this.lblDataDownRate.Foreground = this.colorNormalBlack;
                this.generalStatusUC.SetNetworkUse("Utilisation normal du réseau.");
            }


            if ( data.CurrentUploadRate > Octet.FromMega(2) )
            {
                this.lblDataUpRate.Foreground = this.colorHighWarning;
            }
            else if ( data.CurrentUploadRate > Octet.FromKilo(500) )
            {
                this.lblDataUpRate.Foreground = this.colorWarning;
            }
            else
            {
                this.lblDataUpRate.Foreground = this.colorNormalBlack;
            }

            double percent = Math.Round(data.GetTotal() / data.TrafficMaxLimit * 100, 2);

            this.SetNotifyIconTextData($"{data.GetTotal().ToString(false)} / {data.TrafficMaxLimit.ToString()} \n({percent}%)");

            if ( percent >= 95 )
            {
                Background = this.colorHighWarning;
                this.generalStatusUC.Background = this.colorHighWarning;
            }
            else if ( percent >= 75 )
            {
                Background = this.colorWarning;
                this.generalStatusUC.Background = this.colorWarning;
            }
            else
            {
                Background = this.colorGood;
                this.generalStatusUC.Background = this.colorGood;
            }

            int daysInMonth = GetDaysBetween(DateTime.Now, DateTime.Now.AddMonths(1));

            int startDay = data.StartDay;
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

            //At the start, the data per day we have
            Octet dataPerDay = data.TrafficMaxLimit / daysInMonth;

            //Now, the data remaining per remaining day
            Octet dataRemainPerDay = (data.TrafficMaxLimit - data.GetTotal()) / remainingDays;


            this.lblDataPerDayStart.Content = dataPerDay.ToString();

            this.lblDataPerDayRemain.Content = dataRemainPerDay.ToString();

            if ( dataPerDay > dataRemainPerDay )
            {
                this.lblDataPerDayRemain.Foreground = this.colorHighWarning;
            }
            else
            {
                this.lblDataPerDayRemain.Foreground = this.colorGood;
            }


            int GetDaysBetween (DateTime start, DateTime stop) => (int)(stop - start).TotalDays;

        }

        private void SetNotifyIconTextData (string msg) => this.icon.Text = "WpfData v_" + AppDataFolder.GetVersion() + "\n" + msg;

        private void Icon_Click (object sender, EventArgs e) => this.Show();

        //Application exit
        private void Application_ApplicationExit (object sender, EventArgs e)
        {
            DataLayer.SaveData();
            Properties.Settings.Default.Save();
            this.logger.Log("ApplicationExit EVENT");
            this.icon.Visible = false;
        }

        private void ckbOver_Click (object sender, RoutedEventArgs e)
        {
            if ( this.ckbOver.IsChecked.HasValue )
            {
                Topmost = this.ckbOver.IsChecked.Value;
            }
        }


        //Close window
        protected override void OnClosing (CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btShowGraphs_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                ChartsWindow chartsWindow = new ChartsWindow(this.timeUsage);
                chartsWindow.Topmost = Topmost;
                chartsWindow.Show();
            }
            catch ( Exception ex )
            {
                this.logger.Log("graphs EXCEPTION (" + ex + ")");
                System.Windows.MessageBox.Show("Désolé, une erreur est survenue lors de l'affichage des graphiques. Merci d'en avertir le développeur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btShowLogs_Click (object sender, RoutedEventArgs e)
        {
            AppDataFolder.AccessFolder();
            if ( File.Exists(this.logger.filePath) )
            {
                Process.Start("explorer.exe", "/select,\"" + this.logger.filePath + "\"");
            }
            else
            {
                AppDataFolder.AccessFolder();
                Process.Start("explorer.exe", "\"" + AppDataFolder.FolderPath + "\"");
            }
        }

        private void btUpdate_Click (object sender, RoutedEventArgs e)
        {
            this.logger.Log("EVENT update");

            UpdateCheckInfo info = null;

            if ( ApplicationDeployment.IsNetworkDeployed )
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();
                }
                catch ( DeploymentDownloadException dde )
                {
                    this.logger.Log("EXCEPTION update-check-dde  (" + dde + ")");

                    System.Windows.MessageBox.Show("La nouvelle version ne peut être téléchargée pour le moment.\n\nVérifiez votre connexion au server, ou réessayez plus tard");
                    return;
                }
                catch ( InvalidDeploymentException ide )
                {
                    this.logger.Log("EXCEPTION update-check-ide  (" + ide + ")");

                    System.Windows.MessageBox.Show("Impossible de vérifier le deploiement ClickOnce, la nouvelle version est corrompue.");
                    return;
                }
                catch ( InvalidOperationException ioe )
                {
                    this.logger.Log("EXCEPTION update-check-ioe (" + ioe + ")");

                    System.Windows.MessageBox.Show("Cette aplication ne peut être mise à jour car ce n'est pas un déploiement CkickOnce.");
                    return;
                }

                if ( info.UpdateAvailable )
                {
                    try
                    {
                        ad.Update();
                        System.Windows.Forms.MessageBox.Show("La mise à jour s'est bien passée !");

                        System.Windows.Forms.Application.Restart();
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch ( Exception ex )
                    {
                        this.logger.Log("EXCEPTION update-update  (" + ex + ")");
                        System.Windows.MessageBox.Show("Une erreur est survenue lors de la mise à jour de l'application.");
                    }
                }
                else
                {
                    this.logger.Log("INFO update - LAST VERSION ALREADY INSTALLED");
                    System.Windows.MessageBox.Show("La dernière version est déjà installée !");
                }
            }
            else
            {
                this.logger.Log("INFO update - NOT DEPLOYED APP");
            }
        }
    }
}
