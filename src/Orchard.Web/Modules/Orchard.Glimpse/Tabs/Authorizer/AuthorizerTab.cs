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

        public override string Name => "Authorizer";

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<AuthorizerMessage>();
        }

        public string Key => "glimpse_orchard_authorizer";

        public bool KeysHeadings => false;
    }
}