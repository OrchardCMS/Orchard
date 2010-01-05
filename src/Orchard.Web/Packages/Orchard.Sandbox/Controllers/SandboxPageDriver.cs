using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.Controllers {
    [UsedImplicitly]
    public class SandboxPageDriver : ItemDriver<SandboxPage> {
        public readonly static ContentType ContentType = new ContentType {
            Name = "sandboxpage",
            DisplayName = "Sandbox Page"
        };

        protected override ContentType GetContentType() {
            return ContentType;
        }
        protected override string GetDisplayText(SandboxPage item) {
            return item.Record.Name;
        }

        protected override RouteValueDictionary GetDisplayRouteValues(SandboxPage item) {
            return new RouteValueDictionary(
                new {
                    area = "Orchard.Sandbox",
                    controller = "Page",
                    action = "Show",
                    id = item.ContentItem.Id,
                });
        }

        protected override RouteValueDictionary GetEditorRouteValues(SandboxPage item) {
            return new RouteValueDictionary(
                new {
                    area = "Orchard.Sandbox",
                    controller = "Page",
                    action = "Edit",
                    id = item.ContentItem.Id,
                });
        }

        protected override DriverResult Display(SandboxPage part, string displayType) {
            return Combined(
                ItemTemplate("Items/Sandbox.Page").LongestMatch(displayType, "Summary"),
                PartTemplate(part, "Parts/Sandbox.Page.Title").Location("title"));
        }

        protected override DriverResult Editor(ItemViewModel<SandboxPage> model) {
            return ItemTemplate("Items/Sandbox.Page");
        }

        protected override DriverResult Editor(ItemViewModel<SandboxPage> model, IUpdateModel updater) {
            updater.TryUpdateModel(model, Prefix, null, null);
            return ItemTemplate("Items/Sandbox.Page");
        }
    }
}
