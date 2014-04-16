using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // heavy lifting time...
            var configuration = GlimpseRuntime.Instance.Configuration;
            var resourceEndpoint = configuration.ResourceEndpoint;
            var scripts = configuration.ClientScripts.Where(x => x.Order == order);
            var logger = configuration.Logger;
            var encoder = configuration.HtmlEncoder;
            var resources = configuration.Resources;
            var sb = new StringBuilder();
            var hash = context.Parameters["hash"];
            var resourceAdapter = new BundlingRequestResponseAdaptor(null, sb);

            foreach (var clientScript in scripts)
            {
                
                var dynamicScript = clientScript as IDynamicClientScript;
                if (dynamicScript != null)
                {
                    ////var glimpseRequestId = context.Parameters[GlimpseRequestId];
                    //var path = dynamicScript.GetResourceName();
                    //var resource = resources.FirstOrDefault(r => r.Name.Equals(path, StringComparison.InvariantCultureIgnoreCase));

                    //if (resource == null)
                    //{
                    //    logger.Warn(Resources.RenderClientScriptMissingResourceWarning, clientScript.GetType(), path);
                    //    continue;
                    //}

                    //var resourceResult = resource.Execute(context);

                    //// create new context, pass in string etc.
                    //var generatedScript = resourceResult.Execute();

                    //continue;
                }


                var staticScript = clientScript as IStaticClientScript;
                if (staticScript != null)
                {
                    var scriptPath = staticScript.GetUri(hash);

                    // read from disk
                    // add to string builder
                    // return StringResourceResult from SB

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
