using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Messaging.Services;
using Orchard.UI.Navigation;

namespace Orchard.Messaging {
    [OrchardFeature("Orchard.Messaging.Queuing")]
    public class AdminMenu : Component, INavigationProvider {
        private readonly IMessageQueueManager _messageQueueManager;

        public AdminMenu(IMessageQueueManager messageQueueManager) {
            _messageQueueManager = messageQueueManager;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var queues = _messageQueueManager.GetQueues().ToList();
            builder
                .AddImageSet("messaging")
                .Add(T("Messaging"), "5.0", item => {
                    if (queues.Count == 1) {
                        item.Action("List", "AdminQueue", new { area = "Orchard.Messaging", id = queues.First().Id });
                        item.LinkToFirstChild(false);
                    }
                    else {
                        item.Action("Index", "AdminQueue", new { area = "Orchard.Messaging" });
                    }
                    item.Add(T("Priorities"), "1.1", subItem => subItem
                            .Action("Index", "AdminPriority", new { area = "Orchard.Messaging" }));
                });
        }
    }
}