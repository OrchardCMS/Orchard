using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.Services;

namespace Orchard.DynamicForms.Handlers {
    public class IpAddressFieldHandler : FormElementEventHandlerBase {
        private readonly IClientHostAddressAccessor _clientHostAddressAccessor;

        public IpAddressFieldHandler(IClientHostAddressAccessor clientHostAddressAccessor) {
            _clientHostAddressAccessor = clientHostAddressAccessor;
        }

        public override void GetElementValue(FormElement element, ReadElementValuesContext context) {
            var ipAddressField = element as IpAddressField;

            if (ipAddressField == null)
                return;

            var key = ipAddressField.Name;
            context.Output[key] = _clientHostAddressAccessor.GetClientAddress();
        }
    }
}