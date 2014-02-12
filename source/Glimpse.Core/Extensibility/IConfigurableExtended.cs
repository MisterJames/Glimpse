using Glimpse.Core.Extensibility;

namespace Glimpse.Core.Framework
{
    public interface IConfigurableExtended : IConfigurable
    {
        IConfigurator Configurator { get; }
    }
}