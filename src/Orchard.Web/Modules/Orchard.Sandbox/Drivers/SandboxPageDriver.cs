using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Mvc.ViewModels;
using Orchard.Sandbox.Models;

namespace Orchard.Sandbox.Drivers {
    [UsedImplicitly]
    public class SandboxPageDriver : ContentItemDriver<SandboxPage> {
        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "SandboxPage",
                                                                             DisplayName = "Sandbox Page"
                                                                         };

        protected override ContentType GetContentType() {
            return ContentType;
        }
        protected override string GetDisplayText(SandboxPage item) {
            return item.Record.Name;
        }

        public override RouteValueDictionary GetDisplayRouteValues(SandboxPage item) {
            return new RouteValueDictionary(
                new {
                        area = "Orchard.Sandbox",
                        controller = "Page",
                        action = "Show",
                        id = item.ContentItem.Id,
                    });
        }

        public override RouteValueDictionary GetEditorRouteValues(SandboxPage item) {
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
                ContentItemTemplate("Items/Sandbox.Page").LongestMatch(displayType, "Summary"),
                ContentPartTemplate(part, "Parts/Sandbox.Page.Title").Location("title"));
        }

        protected override DriverResult Editor(ContentItemViewModel<SandboxPage> model) {
            return ContentItemTemplate("Items/Sandbox.Page");
        }

        protected override DriverResult Editor(ContentItemViewModel<SandboxPage> model, IUpdateModel updater) {
            updater.TryUpdateModel(model, Prefix, null, null);
            return ContentItemTemplate("Items/Sandbox.Page");
        }
    }
}