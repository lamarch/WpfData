using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WpfData
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
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
