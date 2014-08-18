using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Services {
    public class DefaultClientIpAddressProvider : IClientIpAddressProvider {
        private readonly IWorkContextAccessor _wca;
        public DefaultClientIpAddressProvider(IWorkContextAccessor wca) {
            _wca = wca;
        }

        public string GetClientIpAddress() {
            var workContext = _wca.GetContext();
            var settings = workContext.CurrentSite.As<ClientIpAddressSettingsPart>();
            var address = workContext.HttpContext.Request.UserHostAddress;

            if (settings.EnableClientIpAddressHeader && !String.IsNullOrWhiteSpace(settings.ClientIpAddressHeaderName)) {
                var headerName = settings.ClientIpAddressHeaderName.Trim();
                var customAddresses = ParseAddresses(workContext.HttpContext.Request.Headers[headerName]).ToArray();

                if (customAddresses.Any())
                    address = customAddresses.First();
            }

            return address;
        }

        private static IEnumerable<string> ParseAddresses(string value) {
            return !String.IsNullOrWhiteSpace(value)
                ? value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : Enumerable.Empty<string>();
        }
    }
}