using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using WpfData.DataStructures;
using WpfData.Requests;
using WpfData.Util;

namespace WpfData.Windows
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly TimeSpan updateWait = new TimeSpan(0, 0, 1);
        private NotifyIcon icon;
        private System.Windows.Forms.MenuItem menuItem_icon_close;

        private List<NetworkMeasure> rtMeasures;
        private List<NetworkMeasure> dailyMeasures;

        private Configuration parserConfig;
        private RequestParser parser;
        private Logger logger;
        private InstanceLocker locker;

        private Task updateTask;
        private bool pause;
        private bool stop;

        public MainWindow ( )
        {


            //config the log system
            this.logger = new Logger();
            this.logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "Application start", App.AppName);


            //config the data layer (serialization)
            DataLayer.Init(logger);
            DataLayer.LoadData();

            dailyMeasures = DataLayer.DailyMeasures ?? new List<NetworkMeasure>();
            rtMeasures = new List<NetworkMeasure>();

            //TODO: rewrite (one more time) the request system
            //TODO: write a configurable request system with JSON
            //config the new parserConfig
            this.parserConfig = new Configuration(new Uri("http://192.168.1.1"), new List<RequestParserFile>()
                {
                    new RequestParserFile("api/monitoring/traffic-statistics", ("CurrentUpload", NetworkMeasureProperty.CurrentUpload), ("CurrentDownload", NetworkMeasureProperty.CurrentDownload), ("CurrentUploadRate", NetworkMeasureProperty.UploadRate), ("CurrentDownloadRate", NetworkMeasureProperty.DownloadRate)),
                    new RequestParserFile("api/monitoring/start_date", ("trafficmaxlimit", NetworkMeasureProperty.TrafficMaxLimit), ("StartDay", NetworkMeasureProperty.StartDay))
                });


            //config the new parser
            this.parser = new RequestParser(parserConfig);
            this.parser.Request();

            //TODO: add task bar on bottom window for network status
            //TODO: better binding on UserControls ?

            #region TaskBarIcon
            //config the taskbar icon menu item
            this.menuItem_icon_close = new System.Windows.Forms.MenuItem("Fermer l'application", new EventHandler((a, b) => System.Windows.Application.Current.Shutdown()));


            //config the task bar icon
            this.icon = new NotifyIcon();
            this.icon.Visible = true;
            this.icon.Icon = Properties.Resources.icon;
            this.icon.Click += this.Icon_Click;
            this.icon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] { this.menuItem_icon_close });
            #endregion

            //config the shutdown
            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            System.Windows.Application.Current.Exit += this.Application_ApplicationExit;


            //TODO: recreate the graphs&charts witht @OxyPlot
            //config the UI
            this.InitializeComponent();
            DataContext = this;
            Title = App.AppName;


            //verify the single instance lock
            locker = new InstanceLocker();
            if ( !App.IsDebugMode )
            {
#pragma warning disable CS0162 // Code inaccessible détecté
                if ( !locker.Check() )
#pragma warning restore CS0162 // Code inaccessible détecté
                {
                    MsgBox("Impossible d'ouvrir plusieurs instances de cette application !", MessageBoxImage.Warning);
                    System.Windows.Application.Current.Shutdown();
                }
                locker.Lock();
            }

            updateTask = Task.Run(( ) => UpdateLoop());
        }

        private async Task UpdateLoop ( )
        {
            while ( !stop )
            {
                //If app is background
                while ( pause )
                {
                    await Task.Delay(100);
                    if ( stop )
                        return;
                }


                DateTime callTime = DateTime.Now;
                await Update();

                //Wait relative to time passed in Update method
                var total = DateTime.Now - callTime;
                if ( total < updateWait )
                {
                    for ( int i = 0; i < 10; i++ )
                    {
                        await Task.Delay((int)(updateWait - total).TotalMilliseconds / 10);
                        if ( stop )
                            return;
                    }
                }
            }
        }

        private int notReadyTimesCount = 0;

        private async Task Update ( )
        {

            NetworkMeasure data;
            Dispatch(( ) => this.Cursor = System.Windows.Input.Cursors.Wait);

            while ( !this.parser.ReadyToParse() )
            {
                SetNetworkStatus($"Attention : Ralentissements bande passante ({++notReadyTimesCount} fois)", Colors.DarkOrange);

                for ( int i = 0; i < 10; i++ )
                {
                    await Task.Delay((int)updateWait.TotalMilliseconds / 100);
                    if ( stop )
                        return;
                }

            }

            SetNetworkStatus("OK");


            if ( notReadyTimesCount > 2 )
            {
                Dispatch(( ) => logger.Log(Logger.LogType.Value, Logger.LogLevel.Info, "method:notReadyTimesCount", notReadyTimesCount.ToString()));
            }
            notReadyTimesCount = 0;

            //Get data from send files
            try
            {
                data = this.parser.Parse();
            }
            catch ( Exception ex )
            {
                Dispatch(( ) => logger.LogException("this:parser.Parse()", ex));
                SetNetworkStatus("Défaut : traitement infos", Colors.Red);
                return;
            }

            //Real time measure
            //
            //Clean old data
            while ( rtMeasures.FirstOrDefault() != null && DateTime.Now - rtMeasures.First().DateTime > new TimeSpan(0, 30, 0) )
            {
                Dispatch(( ) => rtMeasures.RemoveAt(0));
            }

            Dispatch(( ) => this.rtMeasures.Add(data));




            //Daily measure
            //
            //Clean the old items
            while ( dailyMeasures.FirstOrDefault() != null && DateTime.Now - dailyMeasures.First().DateTime > new TimeSpan(40, 0, 0, 0) )
            {
                Dispatch(( ) => dailyMeasures.RemoveAt(0));
            }

            //Add new item
            if ( dailyMeasures.Count < 1 ||
                DateTime.Now - dailyMeasures.Last().DateTime > new TimeSpan(0, 1, 0) )
                Dispatch(( ) => dailyMeasures.Add(data));




            Dispatch(( ) => this.SetWindowTextsData(data));



            //Prepare the next request
            try
            {
                this.parser.Request();
            }
            catch ( Exception ex )
            {
                Dispatch(( ) => logger.LogException("this:parser.Request()", ex));
                SetNetworkStatus("Défaut connexion réseau", Colors.Red);

            }

            Dispatch(( ) => this.Cursor = System.Windows.Input.Cursors.Arrow);

            void SetNetworkStatus (string status, Color? col = null)
            {
                Dispatch(( ) => this.SetNetworkStatus(status, col));
            }

            void Dispatch (Action a)
            {
                Dispatcher.Invoke(a);
            }

        }

        private void SetNetworkStatus(string status, System.Windows.Media.Color? color)
        {
            tbNetworkStatus.Text = "Etat réseau : " + status;
            if ( color.HasValue )
            {
                tbNetworkStatus.Foreground =  new SolidColorBrush(color.Value);
            }
            else
            {
                tbNetworkStatus.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void SetWindowTextsData (NetworkMeasure data)
        {

            double percent = Math.Round(data.TotalMonth / data.TrafficMaxLimit * 100, 2);

            this.generalStatusUC.UpdateTextsData(data);
            this.detailsStatusUC.UpdateTextsData(data, rtMeasures[0]);

            SetNotifyIconTextData($"{data.TotalMonth.ToString(false)} / {data.TrafficMaxLimit.ToString()} \n({percent}%)");

            if ( percent >= 95 )
            {
                Background = App.colorHighWarning;
            }
            else if ( percent >= 75 )
            {
                Background = App.colorWarning;
            }
            else
            {
                Background = App.colorGood;
            }

        }

        private void SetNotifyIconTextData (string msg) => this.icon.Text = App.AppName + "\n" + msg;

        public static void MsgBox (string message, MessageBoxImage image) => System.Windows.MessageBox.Show(message, App.AppName, MessageBoxButton.OK, image);

        private void Icon_Click (object sender, EventArgs e) => this.Activate();

        //Application exit
        private void Application_ApplicationExit (object sender, EventArgs e)
        {
            stop = true;
            DataLayer.SaveData();
            Properties.Settings.Default.Save();

            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "this:Application_ApplicationExit", "Call");
            this.icon.Visible = false;
            locker.Unlock();
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
            pause = true;
            e.Cancel = true;
            this.Hide();
        }

        protected override void OnActivated (EventArgs e)
        {
            pause = false;
            base.OnActivated(e);
        }

        private void btShowGraphs_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                ChartsWindow chartsWindow = new ChartsWindow(this.rtMeasures, dailyMeasures);
                chartsWindow.Topmost = Topmost;
                chartsWindow.Show();*/
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

        private void btShowConfig_Click (object sender, RoutedEventArgs e)
        {

        }
    }
}
