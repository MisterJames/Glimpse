using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.ResourceResult;

namespace Glimpse.Core.Resource
{
    class ClientScriptResource : IResource, IKey
    {
        internal const string InternalName = "glimpse_clientscripts";
        internal const string OrderParameterName = "order";

        public string Name
        {
            get { return InternalName; }
        }

        public IEnumerable<ResourceParameterMetadata> Parameters
        {
            get { return new List<ResourceParameterMetadata> { new ResourceParameterMetadata(OrderParameterName) }; }
        }

        public IResourceResult Execute(IResourceContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var orderName = context.Parameters[OrderParameterName];
            ScriptOrder order;

            if(!Enum.TryParse<ScriptOrder>(orderName, out order))
            {
                return new StatusCodeResourceResult(404, string.Format("Could not resolve ScriptOrder for value provided '{0}'.", orderName));
            }

            var config = GlimpseRuntime.Instance.Configuration;
            var scripts = config.ClientScripts.Where(x => x.Order == order);

            StringBuilder sb = new StringBuilder();

            foreach (var script in scripts)
            {
                // add matches to the sb
            }

            return new CacheControlDecorator(0, CacheSetting.NoCache, new StringResourceResult(@"text/javascript") { Text = sb.ToString() });
        }

        public string Key
        {
            get { return InternalName; }
        }
    }
}
