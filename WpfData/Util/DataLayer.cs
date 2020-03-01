using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Forms;

using WpfData.DataStructures;
using WpfData.Serialization;

namespace WpfData.Util
{
    internal static class DataLayer
    {
        private static BinarySerializer<List<NetworkMeasure>> dailyMeasures;
        private static bool isInit = false;
        private static Logger logger;
        public static void Init (Logger _logger)
        {
            isInit = true;
            logger = _logger;
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "static:DataLayer.Init()", "Call");

            AppDataFolder.Views.Add(new FolderView("data", new FolderView("monthly")));

            dailyMeasures = new BinarySerializer<List<NetworkMeasure>>(AppDataFolder.GetPath(@"data\monthly\.measures"));
            dailyMeasures.Set(new List<NetworkMeasure>()
            {
                new NetworkMeasure()
            });
        }

        public static void LoadData ( )
        {
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "static:DataLayer.LoadData()", "Call");
            if ( !isInit )
            {
                logger.LogException("isInit", new ValueUnavailableException("false"));
                throw new InvalidOperationException("Init method not called !");
            }

            AppDataFolder.AccessFolder();
            try
            {
                dailyMeasures.Load();
            }
            catch ( Exception ex )
            {
                logger.LogException("static,await:dailyMeasures.Load()", ex);
                MessageBox.Show("Une erreur est survenue lors du chargement des données, contactez le dev.");
            }


            if(dailyMeasures.Get() == null )
            {
                dailyMeasures.Set(new List<NetworkMeasure>());
            }
        }

        public static void SaveData ( )
        {
            logger.Log(Logger.LogType.Event, Logger.LogLevel.Info, "static:DataLayer.SaveData()", "Call");

            if ( !isInit )
            {
                logger.LogException("isInit", new ValueUnavailableException("false"));
                throw new InvalidOperationException("Init method not called !");
            }

            AppDataFolder.AccessFolder();
            try
            {
                dailyMeasures.Save();
            }
            catch ( Exception ex )
            {
                logger.LogException("static,await:dailyMeasures.Save()", ex);
                MessageBox.Show("Une erreur est survenue lors de la sauvegarde des données, contactez le dev.");
            }
        }

        public static List<NetworkMeasure> DailyMeasures
        {
            get => dailyMeasures.Get();
            set => dailyMeasures.Set(value);
        }
    }
}
