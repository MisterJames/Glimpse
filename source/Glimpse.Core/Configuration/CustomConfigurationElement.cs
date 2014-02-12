using System;
using System.Configuration;

namespace Glimpse.Core.Configuration
{
    public class CustomConfigurationElement : ConfigurationElement
    {
        public string Key { get; set; }
        public Type Type { get; set; }
        public string ConfigurationContent { get; set; }
    }
}
