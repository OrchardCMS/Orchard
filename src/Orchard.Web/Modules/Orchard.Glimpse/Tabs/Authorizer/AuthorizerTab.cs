using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;

namespace Orchard.Glimpse.Tabs.Authorizer {
    public class AuthorizerTab : TabBase, ITabSetup, IKey, ILayoutControl {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<AuthorizerMessage>().ToList();

            if (!messages.Any()) {
                return "There have been no Authorizer events recorded. If you think there should have been, check that the 'Glimpse for Orchard Authorizer' feature is enabled.";
            }

            return messages;
        }

        public override string Name {
            get { return "Authorizer"; }
        }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<AuthorizerMessage>();
        }

        public string Key {
            get { return "glimpse_orchard_authorizer"; }
        }

        public bool KeysHeadings {
            get { return false; }
        }
    }
}