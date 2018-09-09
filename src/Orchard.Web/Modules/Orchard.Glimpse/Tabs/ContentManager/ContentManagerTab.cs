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

        public override string Name { get { return "Content Manager"; } }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<ContentManagerGetMessage>();
        }

        public string Key { get { return "glimpse_orchard_contentmanager"; } }

        public bool KeysHeadings { get { return false; } }
    }
}