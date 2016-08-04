using Orchard.Forms.Services;
using Orchard.Localization.Services;

namespace Orchard.DynamicForms.Services {
    public class FormElementServices : IFormElementServices {
        public FormElementServices(IFormManager formManager, ICultureManager cultureManager) {
            FormManager = formManager;
            CultureManager = cultureManager;
        }

        public IFormManager FormManager { get; private set; }
        public ICultureManager CultureManager { get; private set; }
    }
}