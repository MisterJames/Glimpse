using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;

namespace Glimpse.Core.Policy
{
    public class ContentTypePolicyConfigurator : IConfigurator
    {
        private ContentTypePolicy ContentTypePolicy { get; set; }

        public ContentTypePolicyConfigurator(ContentTypePolicy contentTypePolicy)
        {
            ContentTypePolicy = contentTypePolicy;
            CustomConfigurationKey = "contentTypes";

            ContentTypePolicy.AddContentType("text/html");
            ContentTypePolicy.AddContentType("application/json");
            ContentTypePolicy.AddContentType("text/plain");
        }

        public string CustomConfigurationKey { get; private set; }

        public void ProcessCustomConfiguration(CustomConfigurationProvider customConfigurationProvider)
        {
            AddContentTypes(customConfigurationProvider.GetMyCustomConfigurationAs<ContentTypePolicyContentTypes>().ContentTypes.Select(contentType => contentType.Value));
        }

        [XmlRoot(ElementName = "contentTypes")]
        public class ContentTypePolicyContentTypes
        {
            [XmlElement(ElementName = "add")]
            public ContentTypePolicyContentType[] ContentTypes;
        }

        public class ContentTypePolicyContentType
        {
            [XmlAttribute("contentType")]
            public string Value { get; set; }
        }

        public void AddContentTypes(IEnumerable<string> contentTypes)
        {
            foreach (var contentType in contentTypes)
            {
                ContentTypePolicy.AddContentType(contentType);
            }
        }
    }
}