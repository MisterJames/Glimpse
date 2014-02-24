using System;
using System.IO;
using System.Xml.Serialization;
using Glimpse.Core.Framework;

namespace Glimpse.Core.Extensibility
{
    /// <summary>
    /// Provides custom configuration to <see cref="IConfigurator"/> instances
    /// </summary>
    public class CustomConfigurationProvider
    {
        private string CustomConfiguration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConfigurationProvider" />
        /// </summary>
        public CustomConfigurationProvider(string customConfiguration)
        {
            if (string.IsNullOrEmpty(customConfiguration))
            {
                throw new ArgumentException("is null or empty", "customConfiguration");
            }

            CustomConfiguration = customConfiguration;
        }

        /// <summary>
        /// Allows <see cref="IConfigurator"/> instances to request there custom configuration by specifying how they want
        /// to get it, or as a plain string value or by specifying a type that can be deserialized from the string 
        /// by the <see cref="XmlSerializer"/>
        /// </summary>
        /// <typeparam name="TCustomConfigurationType"></typeparam>
        /// <returns></returns>
        public TCustomConfigurationType GetCustomConfigurationAs<TCustomConfigurationType>()
            where TCustomConfigurationType : class
        {
            if (typeof(TCustomConfigurationType) == typeof(string))
            {
                return CustomConfiguration as TCustomConfigurationType;
            }

            var xmlSerializer = new XmlSerializer(typeof(TCustomConfigurationType));
            using (var stringReader = new StringReader(CustomConfiguration))
            {
                return (TCustomConfigurationType)xmlSerializer.Deserialize(stringReader);
            }
        }
    }
}