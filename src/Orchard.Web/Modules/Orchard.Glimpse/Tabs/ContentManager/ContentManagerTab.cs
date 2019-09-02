using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using System.Linq;

namespace Orchard.Glimpse.Tabs.ContentManager {
    public class ContentManagerTab : TabBase, ITabSetup, IKey, ILayoutControl {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<ContentManagerGetMessage>().ToList();

            if (!messages.Any()) {
                return null;
            }

            return messages;
        }

        public override string Name => "Content Manager";

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<ContentManagerGetMessage>();
        }

        public string Key => "glimpse_orchard_contentmanager";

        public bool KeysHeadings => false;
    }
}