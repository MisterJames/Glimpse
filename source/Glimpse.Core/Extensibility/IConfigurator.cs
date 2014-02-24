using Glimpse.Core.Extensibility;

namespace Glimpse.Core.Framework
{
    /// <summary>
    /// Represents a configurator
    /// </summary>
    public interface IConfigurator
    {
        /// <summary>
        /// Gets the name of the configuration element which the configurator wants to process
        /// </summary>
        string CustomConfigurationKey { get; }

        /// <summary>
        /// Will be called when custom configuration is available for the given custom configuration key
        /// </summary>
        /// <param name="customConfigurationProvider">The custom configuration provider</param>
        void ProcessCustomConfiguration(CustomConfigurationProvider customConfigurationProvider);
    }
}