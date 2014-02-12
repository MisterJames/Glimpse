using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;

namespace Glimpse.Core.Configuration
{
    public class CustomConfigurationElementCollection : ConfigurationElement
    {
        private List<CustomConfigurationElement> CustomConfigurationElements { get; set; }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            CustomConfigurationElements = new List<CustomConfigurationElement>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(reader.ReadOuterXml());

            foreach (XmlNode element in doc.DocumentElement.ChildNodes)
            {
                var configurationElement = new CustomConfigurationElement();
                configurationElement.Key = element.Name;

                XmlAttribute typeAttribute = element.Attributes["type"];
                if (typeAttribute != null)
                {
                    configurationElement.Type = (Type)new TypeConverter().ConvertFrom(null, null, typeAttribute.Value);
                }

                configurationElement.ConfigurationContent = element.OuterXml;
                CustomConfigurationElements.Add(configurationElement);
            }
        }

        public IEnumerable<CustomConfigurationElement> CustomConfigurationsForKey(string configurationKey)
        {
            return CustomConfigurationElements.Where(
                customConfigurationElement => string.Equals(customConfigurationElement.Key, configurationKey, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
