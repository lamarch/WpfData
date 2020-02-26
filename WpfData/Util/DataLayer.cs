using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Forms;

namespace WpfData.Util
{
    internal static class DataLayer
    {
        private static DataSerializer<MonthlyData> monthlyData;
        private static bool isInit = false;
        private static Logger logger;
        public static void Init ( Logger _logger )
        {
            isInit = true;
            logger = _logger;
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "static:DataLayer.Init()", "Call");

            AppDataFolder.Views.Add(new FolderView("data", new FolderView("monthly")));
            monthlyData = new DataSerializer<MonthlyData>(AppDataFolder.GetPath(@"data\monthly\.measures"));
            monthlyData.Set(new MonthlyData()
            {
                Measures = new List<MonthlyDataMeasure>()
                {
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123),
                    new MonthlyDataMeasure(DateTime.Now, 53, 123)
                }
            });
        }

        public static async void LoadData ( )
        {
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "static,async:DataLayer.LoadData()", "Call");
            if ( !isInit )
            {
                logger.LogException("isInit", new ValueUnavailableException("false"));
                throw new InvalidOperationException("Init method not called !");
            }

            AppDataFolder.AccessFolder();
            try
            { 
                await monthlyData.Load();
            }catch(Exception ex)
            {
                logger.LogException("static,await:monthlyData.Load()", ex);
                MessageBox.Show("Une erreur est survenue lors du chargement des données, contactez le dev.");
            }
}

        public static async void SaveData ( )
        {
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "static,async:DataLayer.SaveData()", "Call");

            if ( !isInit )
            {
                logger.LogException("isInit", new ValueUnavailableException("false"));
                throw new InvalidOperationException("Init method not called !");
            }

            AppDataFolder.AccessFolder();
            try
            {
                await monthlyData.Save();
            }catch(Exception ex )
            {
                logger.LogException("static,await:monthlyData.Save()", ex);
                MessageBox.Show("Une erreur est survenue lors de la sauvegarde des données, contactez le dev.");
            }
        }
    }
}
