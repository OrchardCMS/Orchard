using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Pages.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Pages.Drivers {
    [UsedImplicitly]
    public class PageDriver : ContentItemDriver<Page> {
        public IOrchardServices Services { get; set; }

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "Page",
                                                                             DisplayName = "Page"
                                                                         };

        public PageDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(Page page) {
            return page.Title;
        }

        public override RouteValueDictionary GetDisplayRouteValues(Page page) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Pages"},
                                                {"Controller", "Page"},
                                                {"Action", "Item"},
                                                {"slug", page.Slug},
                                            };
        }

        public override RouteValueDictionary GetEditorRouteValues(Page page) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Pages"},
                                                {"Controller", "Admin"},
                                                {"Action", "Edit"},
                                                {"id", page.Id},
                                            };
        }

        protected override DriverResult Display(Page page, string displayType) {
            return ContentItemTemplate("Items/Pages.Page").LongestMatch(displayType, "Summary", "SummaryAdmin");
        }

        protected override DriverResult Editor(Page page) {
            return Combined(
                ContentItemTemplate("Items/Pages.Page"),
                ContentPartTemplate(page, "Parts/Pages.Page.Publish").Location("secondary", "1"));
        }

        protected override DriverResult Editor(Page page, IUpdateModel updater) {
            updater.TryUpdateModel(page, Prefix, null, null);

            DateTime scheduled;
            if (DateTime.TryParse(string.Format("{0} {1}", page.ScheduledPublishUtcDate, page.ScheduledPublishUtcTime), out scheduled))
                page.ScheduledPublishUtc = scheduled;

            return Editor(page);
        }
    }
}