using Orchard.AuditTrail.Services;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;

namespace Orchard.DynamicForms.Handlers {
    public class IpAddressFieldHandler : FormElementEventHandlerBase {
        private readonly IClientIpAddressProvider _clientIpAddressProvider;

        public IpAddressFieldHandler(IClientIpAddressProvider clientIpAddressProvider) {
            _clientIpAddressProvider = clientIpAddressProvider;
        }

        public override void GetElementValue(FormElement element, ReadElementValuesContext context) {
            var ipAddressField = element as IpAddressField;

            if (ipAddressField == null)
                return;

            var key = ipAddressField.Name;
            context.Output[key] = _clientIpAddressProvider.GetClientIpAddress();
        }
    }
}