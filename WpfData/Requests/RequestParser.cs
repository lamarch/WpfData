using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using WpfData.DataStructures;

namespace WpfData.Requests
{
    internal class RequestParser
    {
        private Configuration config;
        private HttpClient httpClient = new HttpClient();
        private List<Task<Dictionary<NetworkMeasureProperty, string>>> tasks;

        public RequestParser (Configuration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Request ( )
        {
            tasks = new List<Task<Dictionary<NetworkMeasureProperty, string>>>();
            foreach ( var file in config.files )
            {
                tasks.Add(ParseFile(file, config.netAdress));
            }
        }

        public bool ReadyToParse ( )
        {
            if ( tasks == null || tasks.Count == 0 )
            {
                return false;
            }

            foreach ( var task in tasks )
            {
                if ( !task.IsCompleted )
                {
                    return false;
                }
            }
            return true;
        }

        public NetworkMeasure Parse ( )
        {
            NetworkMeasure measure = new NetworkMeasure();

            foreach ( var task in tasks )
            {
                if ( !task.IsCompleted )
                {
                    task.Wait();
                }

                Dictionary<NetworkMeasureProperty, string> values = task.Result;

                foreach ( var value in values )
                {
                    try
                    {
                        measure.SetProperty(value.Key, value.Value);
                    }
                    catch ( Exception ex )
                    {
                        throw new BadRequestConfigException("Request config node had bad values !", ex);
                    }
                }

            }
            tasks.Clear();
            return measure;
        }

        private async Task<Dictionary<NetworkMeasureProperty, string>> ParseFile (RequestParserFile file, Uri path)
        {
            Dictionary<NetworkMeasureProperty, string> results = new Dictionary<NetworkMeasureProperty, string>();
            Uri adress = new Uri(path.ToString() + file.path);

            string xml = "";

            //catch?
            try
            {
                xml = await httpClient.GetStringAsync(adress);
            }
            catch ( ArgumentNullException ex )
            {
                throw new BadRequestConfigException("Request config adress can not be null !", ex);
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xml);
            }
            catch ( XmlException ex )
            {
                throw new BadRequestConfigException("Request config retrieve bad xml files !", ex);
            }

            foreach ( var item in file.equivs )
            {

                XmlNode node = null;

                try
                {
                    node = doc.SelectSingleNode("//" + item.Key);
                }
                catch ( XPathException ex )
                {
                    throw new BadRequestConfigException("Request config had bad property equiv name (xpath) !", ex);
                }

                if ( node != null )
                {
                    try
                    {
                        results.Add(item.Value, node.InnerText);
                    }
                    catch ( Exception ex )
                    {
                        throw new BadRequestConfigException("Request config had bad property equiv name (node value or property name is null) !", ex);
                    }
                }
                else
                {
                    throw new BadRequestConfigException("Request config had bad property equiv name (node does not exist, bad name) !", null);
                }
            }

            return results;
        }
    }
}
