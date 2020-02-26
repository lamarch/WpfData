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
        private const double maxDataRecordMinutes = 15d;



        public static readonly SolidColorBrush colorNormalWhite = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush colorWarning = new SolidColorBrush(Colors.Orange);
        public static readonly SolidColorBrush colorHighWarning = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush colorNormalBlack = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush colorGood = new SolidColorBrush(Colors.Green);
        public static readonly bool IsDebugMode = false;
        public static string AppName = "WpfData v_" + AppDataFolder.GetVersion();

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

        static MainWindow ( )
        {
#if DEBUG
            IsDebugMode = true;
#endif
        }

        public MainWindow ( )
        {



            //config the log system
            this.logger = new Logger();
            this.logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "this:MainWindow()", "Call");


            //config the window
            this.InitializeComponent();
            DataContext = this;
            Title = AppName;


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


            //config the data list
            timeUsage.CollectionChanged += (a, b) =>
            {
                while ( timeUsage.Count > maxDataRecordMinutes * 60 )
                    timeUsage.RemoveAt(0);
            };

        }

        private int notReadyTimesCount = 0;

        private void timerTick (object sender, EventArgs e)
        {
            NetworkDataUsage data = new NetworkDataUsage();
            Cursor = System.Windows.Input.Cursors.Wait;


            if ( this.requester.IsReady() )
            {
                if(notReadyTimesCount > 0 )
                {
                    logger.Log(Logger.LogType.Value, Logger.LogLevel.Info, "method:notReadyTimesCount", notReadyTimesCount.ToString());
                    notReadyTimesCount = 0;
                }


                List<string> responses = null;
                try
                {
                    responses = this.requester.Get();
                }catch(Exception ex )
                {
                    logger.LogException("this:requester.Get()", ex);
                    this.generalStatusUC.SetNetworkStatus("Impossible de se connecter à la box !", Colors.Red);
                    return;
                }

                


                try
                {
                    this.xmlParser.Parse(data, responses.ToArray());
                }
                catch ( Exception ex )
                {
                    logger.LogException("this:xmlParser.Parse()", ex);
                    this.generalStatusUC.SetNetworkStatus("problème formatage/réseau (0x300)", Colors.Red);
                    return;
                }

                this.timeUsage.Add(data);

                this.SetWindowTextsData(data);
                this.generalStatusUC.SetNetworkStatus("OK");

            }
            else
            {
                generalStatusUC.SetNetworkStatus($"Ralentissements bande passante ({++notReadyTimesCount} fois)");
                return;
            }

            try
            {
                this.requester.GetReady();
            }
            catch ( Exception ex )
            {
                logger.LogException("this:requester.GetReady()", ex);
                this.generalStatusUC.SetNetworkStatus("problème connexion réseau (0x100)", Colors.Red);

            }

            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void SetWindowTextsData (NetworkDataUsage data)
        {

            double percent = Math.Round(data.GetTotal() / data.TrafficMaxLimit * 100, 2);

            this.generalStatusUC.UpdateTextsData(data);
            this.detailsStatusUC.UpdateTextsData(data, timeUsage[0]);

            SetNotifyIconTextData($"{data.GetTotal().ToString(false)} / {data.TrafficMaxLimit.ToString()} \n({percent}%)");

            if ( percent >= 95 )
            {
                Background = colorHighWarning;
            }
            else if ( percent >= 75 )
            {
                Background = colorWarning;
            }
            else
            {
                Background = colorGood;
            }

        }

        private void SetNotifyIconTextData (string msg) => this.icon.Text = AppName + "\n" + msg;

        public static void MsgBox(string message, MessageBoxImage image)
        {
            System.Windows.MessageBox.Show(message, AppName, MessageBoxButton.OK, image);
        }




        private void Icon_Click (object sender, EventArgs e) => this.Show();

        //Application exit
        private void Application_ApplicationExit (object sender, EventArgs e)
        {
            DataLayer.SaveData();
            Properties.Settings.Default.Save();
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "this:Application_ApplicationExit", "Call");
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
                logger.LogException("method:chartsWindow-Initialization", ex);
                MsgBox("Désolé, une erreur est survenue lors de l'affichage des graphiques. Merci d'en avertir le développeur.", MessageBoxImage.Error);
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
            this.logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "this:btUpdate_Click", "Call");

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
                    this.logger.LogException("method:ad.CheckForDetailedUpdate()", dde);

                    MsgBox("La nouvelle version ne peut être téléchargée pour le moment.\n\nVérifiez votre connexion au server, ou réessayez plus tard", MessageBoxImage.Error);
                    return;
                }
                catch ( InvalidDeploymentException ide )
                {
                    this.logger.LogException("method:ad.CheckForDetailedUpdate()", ide);

                    MsgBox("Impossible de vérifier le deploiement ClickOnce, la nouvelle version est corrompue.", MessageBoxImage.Error);
                    return;
                }
                catch ( InvalidOperationException ioe )
                {
                    this.logger.LogException("method:ad.CheckForDetailedUpdate()", ioe);

                    MsgBox("Cette aplication ne peut être mise à jour car ce n'est pas un déploiement CkickOnce.", MessageBoxImage.Error);
                    return;
                }

                if ( info.UpdateAvailable )
                {
                    try
                    {
                        ad.Update();
                        MsgBox("La mise à jour s'est bien passée !", MessageBoxImage.Information);

                        logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "method:ad.Update()", "Update Ended Well");

                        System.Windows.Forms.Application.Restart();
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch ( Exception ex )
                    {
                        logger.LogException("method:ad.Update()", ex);
                        MsgBox("Une erreur est survenue lors de la mise à jour de l'application.", MessageBoxImage.Error);
                    }
                }
                else
                {
                    logger.Log(Logger.LogType.Value, Logger.LogLevel.Info, "method:info.UpdateAvailable", "false");
                    MsgBox("La dernière version est déjà installée !", MessageBoxImage.Exclamation);
                }
            }
            else
            {
                logger.Log(Logger.LogType.Value, Logger.LogLevel.Info, "static:ApplicationDeployment.IsNetworkDeployed", "false");
                MsgBox("Vous ne possedez pas une version de l'application déployée par ClickOnce.", MessageBoxImage.Exclamation);
            }
        }
    }
}
