using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;

namespace Orchard.DynamicForms.Handlers {
    public class UserNameFieldHandler : FormElementEventHandlerBase {
        private readonly IWorkContextAccessor _wca;
        public UserNameFieldHandler(IWorkContextAccessor wca) {
            _wca = wca;
        }

        public override void GetElementValue(FormElement element, ReadElementValuesContext context) {
            var userNameField = element as UserNameField;

            if (userNameField == null)
                return;

            var key = userNameField.Name;
            var currentUser = _wca.GetContext().CurrentUser;
            context.Output[key] = currentUser != null ? currentUser.UserName : null;
        }
    }
}