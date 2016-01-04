using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using System.Linq;

namespace Orchard.Glimpse.Tabs.Parts {
    public class PartTab : TabBase, ITabSetup, IKey {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<PartMessage>().ToList();

            if (!messages.Any()) {
                return "There have been no Content Part events recorded. If you think there should have been, check that the 'Glimpse for Orchard Content Parts' feature is enabled.";
            }

            return messages;
        }

        public override string Name {
            get { return "Parts"; }
        }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<PartMessage>();
        }

        public string Key {
            get { return "glimpse_orchard_parts"; }
        }
    }
}