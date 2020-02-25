using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            logger = _logger;
            logger.Log("DataLayer.Init() EVENT");

            isInit = true;
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
            logger.Log("DataLayer.LoadData() EVENT");
            if ( !isInit )
            {
                logger.Log("DataLayer.LoadData() - isInit EXCEPTION (uninitilized)");
                throw new InvalidOperationException("Init method not called !");
            }

            AppDataFolder.AccessFolder();
            try
            { 
                await monthlyData.Load();
            }catch(Exception ex)
            {
                logger.Log("DataLayer.LoadData() - Load EXCEPTION (" + ex + ")");
                MessageBox.Show("Une erreur est survenue lors du chargement des données, contactez le dev.");
            }
}

        public static async void SaveData ( )
        {
            logger.Log("DataLayer.SaveData() EVENT");
            if ( !isInit )
            {
                logger.Log("DataLayer.SaveData() - isInit EXCEPTION (uninitilized)");
                throw new InvalidOperationException("Init method not called !");
            }

            AppDataFolder.AccessFolder();
            try
            {
                await monthlyData.Save();
            }catch(Exception ex )
            {
                logger.Log("DataLayer.SaveData() - Save EXCEPTION (" + ex + ")");
                MessageBox.Show("Une erreur est survenue lors de la sauvegarde des données, contactez le dev.");
            }
        }
    }
}
