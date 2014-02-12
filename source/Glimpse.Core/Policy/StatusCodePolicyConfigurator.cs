using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Glimpse.Core.Configuration;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;

namespace Glimpse.Core.Policy
{
    public class StatusCodePolicyConfigurator : IConfigurator
    {
        private StatusCodePolicy StatusCodePolicy { get; set; }

        public StatusCodePolicyConfigurator(StatusCodePolicy statusCodePolicy)
        {
            StatusCodePolicy = statusCodePolicy;
            CustomConfigurationKey = "statusCodes";

            StatusCodePolicy.AddStatusCode(200);
            StatusCodePolicy.AddStatusCode(301);
            StatusCodePolicy.AddStatusCode(302);
        }

        public string CustomConfigurationKey { get; private set; }

        /// <summary>
        /// Provides implementations an instance of <see cref="Section" /> to self populate any end user configuration options.
        /// </summary>
        /// <param name="section">The configuration section, <c>&lt;glimpse&gt;</c> from <c>web.config</c>.</param>
        /// <remarks>
        /// Populates the status code white list with values from <c>web.config</c>.
        /// </remarks>
        /// <example>
        /// Configure the status code white list in <c>web.config</c> with the following entries:
        /// <code>
        /// <![CDATA[
        /// <glimpse defaultRuntimePolicy="On" endpointBaseUri="~/Glimpse.axd">
        ///     <runtimePolicies>
        ///         <statusCodes>
        ///             <!-- <clear /> clear to reset defaults -->
        ///             <add statusCode="{code}" />
        ///         </statusCodes>
        ///     </runtimePolicies>
        /// </glimpse>
        /// ]]>
        /// </code>
        /// </example>
        public void Configure(Section section)
        {
            foreach (StatusCodeElement item in section.RuntimePolicies.StatusCodes)
            {
                StatusCodePolicy.AddStatusCode(item.StatusCode);
            }
        }

        public void ProcessCustomConfiguration(CustomConfigurationProvider customConfigurationProvider)
        {
            AddStatusCodes(customConfigurationProvider.GetMyCustomConfigurationAs<StatusCodePolicyStatusCodes>().StatusCodes.Select(statusCode => statusCode.Value));
        }
       
        [XmlRoot(ElementName = "statusCodes")]
        public class StatusCodePolicyStatusCodes
        {
            [XmlElement(ElementName = "add")]
            public StatusCodePolicyStatusCode[] StatusCodes;
        }

        public class StatusCodePolicyStatusCode
        {
            [XmlAttribute("statusCode")]
            public int Value { get; set; }
        }

        public void AddStatusCodes(IEnumerable<int> statusCodes)
        {
            foreach (var statusCode in statusCodes)
            {
                StatusCodePolicy.AddStatusCode(statusCode);
            }
        }
    }
}