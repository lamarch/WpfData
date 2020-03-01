using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using WpfData.Util;

namespace WpfData
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {

        internal const string updatePath = @"\\PM54\Users\Xavier\source\repos\WpfData\WpfData\publish\setup.exe";
        internal const double maxDataRecordMinutes = 30d;

#if DEBUG
        internal const bool IsDebugMode = true;
#else
        internal const bool IsDebugMode = false;
#endif

        internal static readonly SolidColorBrush colorNormalWhite = new SolidColorBrush(Colors.White);
        internal static readonly SolidColorBrush colorWarning = new SolidColorBrush(Colors.Orange);
        internal static readonly SolidColorBrush colorHighWarning = new SolidColorBrush(Colors.Red);
        internal static readonly SolidColorBrush colorNormalBlack = new SolidColorBrush(Colors.Black);
        internal static readonly SolidColorBrush colorGood = new SolidColorBrush(Colors.Green);
        internal static string AppName = "WpfData v_" + AppDataFolder.GetVersion();


        protected override void OnStartup (StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupExceptionHandling();
        }

        private void SetupExceptionHandling ( )
        {
            AppDomain.CurrentDomain.UnhandledException += this.UnhandledException;
            DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException (object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                File.AppendAllText(AppDataFolder.GetPath("FatalError.txt"), DateTime.Now.ToString() + " - FATAL ERROR TaskScheduler - UnobservedTaskException (by " + sender + " : " + e.Exception + "\n\n\n");
            }
            catch
            {
                MessageBox.Show("Une erreur importante est survenue !\n" + e);
            }
        }
        private void App_DispatcherUnhandledException (object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                File.AppendAllText(AppDataFolder.GetPath("FatalError.txt"), DateTime.Now.ToString() + " - FATAL ERROR App - DispatcherUnhandledException (by " + sender + " : " + e.Exception + "\n\n\n");
            }
            catch
            {
                MessageBox.Show("Une erreur importante est survenue !\n" + e);
            }
        }
        private void UnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                File.AppendAllText(AppDataFolder.GetPath("FatalError.txt"), DateTime.Now.ToString() + " - FATAL ERROR AppDomain - UnhandledException (by " + sender + " : " + e.ExceptionObject + "\n\n\n");
            }
            catch
            {
                MessageBox.Show("Une erreur importante est survenue !\n" + e);
            }
        }
    }
}
