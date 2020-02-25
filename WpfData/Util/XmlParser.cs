using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Security;

namespace WpfData
{
    public class XmlParser<T> where T : IXmlParserObject
    {

        private List<string> propNames;
        public XmlParser (params string[] propNames)
        {
            this.propNames = propNames.ToList();
        }

        public void Parse (T obj, params string[] xmls)
        {
            List<string> propEdited = new List<string>();
            foreach ( string xml in xmls )
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                foreach(string propName in propNames )
                {
                    XmlNode node = doc.SelectSingleNode("//" + propName);
                    if(node != null)
                    {
                        obj.SetProperty(propName, node.InnerText);
                        if ( propEdited.Contains(propName) )
                            throw new Exception();
                        propEdited.Add(propName);
                    }
                }
            }
        }
    }

    public interface IXmlParserObject
    {
        void SetProperty (string name, string val);
    }
}
