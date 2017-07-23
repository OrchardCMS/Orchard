using System;
using System.Configuration;
using System.Linq;
using System.Web;
using Glimpse.Core.Extensibility;

namespace Orchard.Glimpse.GlimpseExtensions {
    public class WhitelistedIpAddressesSecurityPolicy : IRuntimePolicy {
        public RuntimePolicy Execute(IRuntimePolicyContext policyContext) {
            var request = HttpContext.Current.Request;
            var whitelistedIpAddressesValue = ConfigurationManager.AppSettings["Orchard.Glimpse:WhitelistedIpAddresses"] ?? string.Empty;
            var whitelistedIpAddresses = whitelistedIpAddressesValue.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);

            if (request.IsLocal || !whitelistedIpAddresses.Any()) {
                return RuntimePolicy.On;
            }

            return whitelistedIpAddresses.Contains(request.UserHostAddress) ? RuntimePolicy.On : RuntimePolicy.Off;
        }

        public RuntimeEvent ExecuteOn => RuntimeEvent.EndRequest | RuntimeEvent.ExecuteResource;
    }
}