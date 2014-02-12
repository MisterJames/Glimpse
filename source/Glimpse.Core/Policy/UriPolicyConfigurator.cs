using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;

namespace Glimpse.Core.Policy
{
    public class UriPolicyConfigurator : IConfigurator
    {
        private UriPolicy UriPolicy { get; set; }

        public UriPolicyConfigurator(UriPolicy uriPolicy)
        {
            UriPolicy = uriPolicy;
            CustomConfigurationKey = "uris";
            
            UriPolicy.AddUriRegex(new Regex("__browserLink/requestData"));
        }

        public string CustomConfigurationKey { get; private set; }

        public void ProcessCustomConfiguration(CustomConfigurationProvider customConfigurationProvider)
        {
            AddUris(customConfigurationProvider.GetMyCustomConfigurationAs<UriPolicyUris>().Uris.Select(uri=> new Regex(uri.RegexPattern)));
        }

        [XmlRoot(ElementName = "uris")]
        public class UriPolicyUris
        {
            [XmlElement(ElementName = "add")]
            public UriPolicyUri[] Uris;
        }

        public class UriPolicyUri
        {
            [XmlAttribute("regex")]
            public string RegexPattern { get; set; }
        }

        public void AddUris(IEnumerable<Regex> uris)
        {
            foreach (var uri in uris)
            {
                UriPolicy.AddUriRegex(uri);
            }
        }
    }
}