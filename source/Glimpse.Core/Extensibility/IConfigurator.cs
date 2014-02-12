using Glimpse.Core.Extensibility;

namespace Glimpse.Core.Framework
{
    public interface IConfigurator
    {
        string CustomConfigurationKey { get; }
        void ProcessCustomConfiguration(CustomConfigurationProvider customConfigurationProvider);
    }
}