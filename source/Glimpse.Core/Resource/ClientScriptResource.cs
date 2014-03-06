using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.ResourceResult;
using Tavis.UriTemplates;

namespace Glimpse.Core.Resource
{
    class ClientScriptResource : IResource, IKey
    {
        internal const string InternalName = "glimpse_clientscripts";
        internal const string GlimpseRequestId = "glimpse_request_id";
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
            // verify that the context is populated
            if (context == null) 
            {
                throw new ArgumentNullException("context");
            }

            // check for the order parameter
            var orderName = context.Parameters[OrderParameterName];
            if (string.IsNullOrEmpty(orderName))
            {
                throw new ArgumentNullException(OrderParameterName);
            }

            // ensure that we can resolve the order name to the enum
            ScriptOrder order;
            try
            {
                // have to use try..catch instead of Enum.TryParse to support NET35
                order = (ScriptOrder)Enum.Parse(typeof(ScriptOrder), orderName);
            }
            catch (ArgumentException)
            {
                return new StatusCodeResourceResult(404, string.Format("Could not resolve ScriptOrder for value provided '{0}'.", orderName));
            }

            // heavy lifting time...
            var configuration = GlimpseRuntime.Instance.Configuration;
            var resourceEndpoint = configuration.ResourceEndpoint;
            var scripts = configuration.ClientScripts.Where(x => x.Order == order);
            var logger = configuration.Logger;
            var encoder = configuration.HtmlEncoder;
            var resources = configuration.Resources;
            var sb = new StringBuilder();

            foreach (var clientScript in scripts)
            {
                
                var dynamicScript = clientScript as IDynamicClientScript;
                if (dynamicScript != null)
                {
                    //var glimpseRequestId = context.Parameters[GlimpseRequestId];
                    var path = dynamicScript.GetResourceName();
                    var resource = resources.FirstOrDefault(r => r.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase));

                    if (resource == null)
                    {
                        logger.Warn(Resources.RenderClientScriptMissingResourceWarning, clientScript.GetType(), path);
                        continue;
                    }

                    var uriTemplate = resourceEndpoint.GenerateUriTemplate(resource, configuration.EndpointBaseUri, logger);

                    // cheating, not using any params at this point, I know...
                    var uri = new UriTemplate(uriTemplate).Resolve();
                    // what to do here? 

                    continue;
                }

                var staticScript = clientScript as IStaticClientScript;
                if (staticScript != null)
                {
                    // note: don't currently have access through configuration for version
                    // question: do we need version when resolving a static resource?
                    // suggestion: refactor GetUri to be GetPath...differentiate between local static files
                    //             and, for example, CDN files that need to be included such as jQuery
                    var scriptPath = staticScript.GetUri("");
                    continue;
                }

                logger.Warn(Core.Resources.RenderClientScriptImproperImplementationWarning, clientScript.GetType());
            }

            // test for correct wiring...
            sb.AppendLine(@"<script type='text/javascript'>alert('foo');</script>");

            return new CacheControlDecorator(0, CacheSetting.NoCache, new StringResourceResult(@"text/javascript") { Text = sb.ToString() });
        }

        private static UriTemplate SetParameters(UriTemplate template,
            IEnumerable<KeyValuePair<string, string>> nameValues)
        {
            if (nameValues == null)
            {
                return template;
            }

            foreach (var pair in nameValues)
            {
                template.SetParameter(pair.Key, pair.Value);
            }

            return template;
        }


        public string Key
        {
            get { return InternalName; }
        }
    }
}
