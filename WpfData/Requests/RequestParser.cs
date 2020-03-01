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
        private RequestParserConfig config;
        private HttpClient httpClient = new HttpClient();
        private List<Task<Dictionary<NetworkMeasureProperty, string>>> tasks;

        public RequestParser (RequestParserConfig config)
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
                    throw new Exception("A task was not completed !");
                }

                Dictionary<NetworkMeasureProperty, string> values = null;

                values = task.Result;

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

        private async Task<Dictionary<NetworkMeasureProperty, string>> ParseFile (RequestParserFile file, string path)
        {
            Dictionary<NetworkMeasureProperty, string> results = new Dictionary<NetworkMeasureProperty, string>();
            string adress = path + file.name;

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
