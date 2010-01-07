using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Pages.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Pages.Controllers {
    [UsedImplicitly]
    public class PageDriver : ContentItemDriver<Page> {
        public readonly static ContentType ContentType = new ContentType {
            Name = "page",
            DisplayName = "Page"
        };

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(Page page) {
            return page.Title;
        }

        protected override RouteValueDictionary GetDisplayRouteValues(Page page) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Pages"},
                                                {"Controller", "Page"},
                                                {"Action", "Item"},
                                                {"slug", page.Slug},
                                            };
        }

        protected override RouteValueDictionary GetEditorRouteValues(Page page) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Pages"},
                                                {"Controller", "Admin"},
                                                {"Action", "Edit"},
                                                {"pageSlug", page.Slug},
                                            };
        }

        protected override DriverResult Display(Page page, string displayType) {
            return ContentItemTemplate("Items/Pages.Page").LongestMatch(displayType, "Summary", "SummaryAdmin");
        }

        protected override DriverResult Editor(Page page) {
            return Combined(
                ContentItemTemplate("Items/Pages.Page"),
                ContentPartTemplate(page, "Parts/Pages.Page.Fields").Location("primary", "1"),
                ContentPartTemplate(page, "Parts/Pages.Page.Publish").Location("secondary", "1"));
        }

        protected override DriverResult Editor(Page page, IUpdateModel updater) {
            updater.TryUpdateModel(page, Prefix, null, null);
            return Editor(page);
        }
    }
}
