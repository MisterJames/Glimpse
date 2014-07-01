using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.ResourceResult;
using Tavis.UriTemplates;

namespace Glimpse.Core.Resource
{

    public class ScriptBundlingResource : IResource, IKey
    {
        private class BundlingRequestResponseAdaptor : IRequestResponseAdapter
        {
            private IRequestResponseAdapter Inner { get; set; }

            private StringBuilder Buffer { get; set; }

            public BundlingRequestResponseAdaptor(IRequestResponseAdapter inner, StringBuilder buffer)
            {
                Inner = inner;
                Buffer = buffer;
            }

            public object RuntimeContext
            {
                get { return Inner.RuntimeContext; }
            }

            public IRequestMetadata RequestMetadata
            {
                get { return Inner.RequestMetadata; }
            }

            public void SetHttpResponseHeader(string name, string value)
            {
                Inner.SetHttpResponseHeader(name, value);
            }

            public void SetHttpResponseStatusCode(int statusCode)
            {
                Inner.SetHttpResponseStatusCode(statusCode);
            }

            public void SetCookie(string name, string value)
            {
                Inner.SetCookie(name, value);
            }

            public void InjectHttpResponseBody(string htmlSnippet)
            {
                Inner.InjectHttpResponseBody(htmlSnippet);
            }

            public void WriteHttpResponse(byte[] content)
            {
                // convert to string, push to buffer
                //var message = System.Text.Encoding.Convert(Encoding.UTF8, )
                throw new NotImplementedException();
            }

            public void WriteHttpResponse(string content)
            {
                Buffer.Append(content);
            }
        }


        internal const string InternalName = "glimpse_scriptbundling";
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

            // locate scripts and add to response
            var configuration = GlimpseRuntime.Instance.Configuration;
            var scripts = configuration.ClientScripts.Where(x => x.Order == order);
            var logger = configuration.Logger;
            var sb = new StringBuilder();
            var hash = context.Parameters["hash"];

            foreach (var clientScript in scripts)
            {
                
                var staticScript = clientScript as IStaticClientScript;
                if (staticScript != null)
                {
                    var scriptPath = staticScript.GetUri(hash);

                    var scriptContents = File.ReadAllText(scriptPath);
                    sb.Append(scriptContents);
                    
                    continue;
                }

                logger.Warn(Core.Resources.RenderClientScriptImproperImplementationWarning, clientScript.GetType());
            }

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
