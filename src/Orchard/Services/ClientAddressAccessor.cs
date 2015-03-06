using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.Services {
    /// <summary>
    /// Provides access to the client host address.
    /// </summary>
    public class ClientAddressAccessor : IClientAddressAccessor {
        private readonly IWorkContextAccessor _wca;
        public ClientAddressAccessor(IWorkContextAccessor wca) {
            _wca = wca;
        }

        /// <summary>
        /// Indicates whether the client host address should be read from an HTTP header, specified via <see cref="ClientAddressHeaderName"/>
        /// </summary>
        public bool EnableClientAddressHeader { get; set; }

        /// <summary>
        /// The HTTP header name to read the client host address from if <see cref="EnableClientAddressHeader"/> is set to true.
        /// If the specified header was not found, the system will fall back to the user host address as provided by the Request object.
        /// </summary>
        public string ClientAddressHeaderName { get; set; }

        public string GetClientAddress() {
            var request = _wca.GetContext().HttpContext.Request;

            if (EnableClientAddressHeader && !String.IsNullOrWhiteSpace(ClientAddressHeaderName)) {
                var headerName = ClientAddressHeaderName.Trim();
                var customAddresses = ParseAddresses(request.Headers[headerName]).ToArray();

                if (customAddresses.Any())
                    return customAddresses.First();
            }

            return request.UserHostAddress;
        }

        private static IEnumerable<string> ParseAddresses(string value) {
            return !String.IsNullOrWhiteSpace(value)
                ? value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : Enumerable.Empty<string>();
        }
    }
}