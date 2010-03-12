using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Services;
using Orchard.Localization;
using Orchard.Pages.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Pages.Services;
using Orchard.UI.Notify;

namespace Orchard.Pages.Drivers {
    [UsedImplicitly]
    public class PageDriver : ContentItemDriver<Page> {
        public IOrchardServices Services { get; set; }
        private readonly IPageService _pageService;
        private readonly IRoutableService _routableService;

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "page",
                                                                             DisplayName = "Page"
                                                                         };

        public PageDriver(IOrchardServices services, IPageService pageService, IRoutableService routableService) {
            Services = services;
            _pageService = pageService;
            _routableService = routableService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

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
            return Combined(
                ContentItemTemplate("Items/Pages.Page").LongestMatch(displayType, "Summary", "SummaryAdmin"),
                ContentPartTemplate(page, "Parts/Pages.Page.Metadata").Location("primary:metadata"));
        }

        protected override DriverResult Editor(Page page) {
            return Combined(
                ContentItemTemplate("Items/Pages.Page"),
                ContentPartTemplate(page, "Parts/Pages.Page.Publish").Location("secondary", "1"));
        }

        protected override DriverResult Editor(Page page, IUpdateModel updater) {
            updater.TryUpdateModel(page, Prefix, null, null);

            //todo: (heskew) something better needs to be done with this...still feels shoehorned in here
            ProcessSlug(page, updater);

            DateTime scheduled;
            if (DateTime.TryParse(string.Format("{0} {1}", page.ScheduledPublishUtcDate, page.ScheduledPublishUtcTime), out scheduled))
                page.ScheduledPublishUtc = scheduled;

            return Editor(page);
        }

        private void ProcessSlug(Page page, IUpdateModel updater) {
            _routableService.FillSlug(page.As<RoutableAspect>(), Slugify);

            if (string.IsNullOrEmpty(page.Slug)) {
                return;

                // OR

                //updater.AddModelError("Routable.Slug", T("The slug is required.").ToString());
                //return;
            }

            if (!Regex.IsMatch(page.Slug, @"^[^/:?#\[\]@!$&'()*+,;=\s](?(?=/)/[^/:?#\[\]@!$&'()*+,;=\s]|[^:?#\[\]@!$&'()*+,;=\s])*$")) {
                updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your slugs: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\". No spaces are allowed (please use dashes or underscores instead).").ToString());
                return;
            }

            var slugsLikeThis = _pageService.Get(PageStatus.Published).Where(
                p => p.Slug.StartsWith(page.Slug, StringComparison.OrdinalIgnoreCase) &&
                     p.Id != page.Id).Select(p => p.Slug);

            if (slugsLikeThis.Count() > 0) {
                //todo: (heskew) need better messages
                Services.Notifier.Warning(T("A different page is already published with this same slug."));

                if (page.ContentItem.VersionRecord == null || page.ContentItem.VersionRecord.Published) {
                    var originalSlug = page.Slug;
                    //todo: (heskew) make auto-uniqueness optional
                    page.Slug = _routableService.GenerateUniqueSlug(page.Slug, slugsLikeThis);

                    //todo: (heskew) need better messages
                    if (originalSlug != page.Slug)
                        Services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously published page so this page now has the slug \"{1}\"",
                                                    originalSlug, page.Slug));
                }
            }
        }

        private static string Slugify(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                //todo: (heskew) improve - just doing multi-pass regex replaces for now with the simple rules of
                // (1) can't begin with a '/', (2) can't have adjacent '/'s and (3) can't have these characters
                var startsoffbad = new Regex(@"^[\s/]+");
                var slashhappy = new Regex("/{2,}");
                var dissallowed = new Regex(@"[:?#\[\]@!$&'()*+,;=\s]+");

                value = startsoffbad.Replace(value, "-");
                value = slashhappy.Replace(value, "/");
                value = dissallowed.Replace(value, "-");
                value = value.Trim('-');

                if (value.Length > 1000)
                    value = value.Substring(0, 1000);
            }

            return value;
        }
    }
}