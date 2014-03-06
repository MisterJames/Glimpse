using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Resource;
using Glimpse.Core.ResourceResult;
using Tavis.UriTemplates;

namespace Glimpse.Core.Framework
{
    /// <summary>
    /// Generator of Glimpse script tags
    /// </summary>
    public static class GlimpseScriptTagsGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="glimpseRequestId">The Glimpse request Id for the request for which script tags must be generated</param>
        /// <param name="configuration">A <see cref="IGlimpseConfiguration"/></param>
        /// <param name="glimpseRuntimeVersion">The version of the glimpse runtime</param>
        /// <returns>The generated script tags</returns>
        public static string Generate(Guid glimpseRequestId, IGlimpseConfiguration configuration, string glimpseRuntimeVersion)
        {
            var encoder = configuration.HtmlEncoder;
            var clientScripts = configuration.ClientScripts;

            var stringBuilder = new StringBuilder();

            var scripts = clientScripts.OrderBy(cs => cs.Order).GroupBy(x => x.Order).ToDictionary(x => x.Key, y => y.ToList());
            foreach (var scriptGroup in scripts)
            {
                switch (scriptGroup.Key)
                {
                    case ScriptOrder.RequestDataScript:
                        stringBuilder.AppendFormat(@"<script type='text/javascript' src='/glimpse.axd?n={0}&order={1}&{2}={3}'></script>", ClientScriptResource.InternalName, scriptGroup.Key, ClientScriptResource.GlimpseRequestId, glimpseRequestId);
                        break;
                    case ScriptOrder.ClientInterfaceScript:
                        stringBuilder.AppendFormat(@"<script type='text/javascript' src='/glimpse.axd?n={0}&order={1}'></script>", ClientScriptResource.InternalName, scriptGroup.Key);
                        break;
                    case ScriptOrder.RequestMetadataScript:
                        stringBuilder.AppendFormat(@"<script type='text/javascript' src='/glimpse.axd?n={0}&order={1}'></script>", ClientScriptResource.InternalName, scriptGroup.Key);
                        break;
                    default:
                        // the three above are internal, all other cases use the following
                        stringBuilder.AppendFormat(@"<script type='text/javascript' src='/glimpse.axd?n={0}&order={1}'></script>", ClientScriptResource.InternalName, scriptGroup.Key);
                        break;
                }
            }

            return stringBuilder.ToString();
        }


    }
}