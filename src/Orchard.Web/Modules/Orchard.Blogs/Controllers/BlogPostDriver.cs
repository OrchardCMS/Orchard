using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Services;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Blogs.Controllers {
    [UsedImplicitly]
    public class BlogPostDriver : ContentItemDriver<BlogPost> {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;
        private readonly IRoutableService _routableService;
        private readonly IOrchardServices _orchardServices;

        public readonly static ContentType ContentType = new ContentType {
            Name = "blogpost",
            DisplayName = "Blog Post"
        };

        public BlogPostDriver(IBlogService blogService, IBlogPostService blogPostService, IRoutableService routableService, IOrchardServices orchardServices) {
            _blogService = blogService;
            _blogPostService = blogPostService;
            _routableService = routableService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(BlogPost post) {
            return post.Title;
        }

        protected override RouteValueDictionary GetDisplayRouteValues(BlogPost post) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPost"},
                                                {"Action", "Item"},
                                                {"blogSlug", post.Blog.Slug},
                                                {"postSlug", post.Slug},
                                            };
        }

        protected override RouteValueDictionary GetEditorRouteValues(BlogPost post) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPost"},
                                                {"Action", "Edit"},
                                                {"blogSlug", post.Blog.Slug},
                                                {"postSlug", post.Slug},
                                            };
        }

        protected override DriverResult Display(BlogPost post, string displayType) {
            return ContentItemTemplate("Items/Blogs.BlogPost").LongestMatch(displayType, "Summary", "SummaryAdmin");
        }

        protected override DriverResult Editor(BlogPost post) {
            return Combined(
                ContentItemTemplate("Items/Blogs.BlogPost"),
                ContentPartTemplate(post, "Parts/Blogs.BlogPost.Publish").Location("secondary", "1"));
        }

        protected override DriverResult Editor(BlogPost post, IUpdateModel updater) {
            updater.TryUpdateModel(post, Prefix, null, null);

            //todo: (heskew) something better needs to be done with this...still feels shoehorned in here
            ProcessSlug(post, updater);

            DateTime scheduled;
            if (DateTime.TryParse(string.Format("{0} {1}", post.ScheduledPublishUtcDate, post.ScheduledPublishUtcTime), out scheduled))
                post.ScheduledPublishUtc = scheduled;

            return Editor(post);
        }

        private void ProcessSlug(BlogPost post, IUpdateModel updater) {
            _routableService.FillSlug(post.As<RoutableAspect>());

            if (string.IsNullOrEmpty(post.Slug)) {
                return;

                // OR

                //updater.AddModelError("Routable.Slug", T("The slug is required.").ToString());
                //return;
            }

            if (!Regex.IsMatch(post.Slug, @"^[^/:?#\[\]@!$&'()*+,;=\s]+$")) {
                //todo: (heskew) get rid of the hard-coded prefix
                updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your slugs: \"/\", \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\". No spaces are allowed (please use dashes or underscores instead).").ToString());
                return;
            }

            var slugsLikeThis = _blogPostService.Get(post.Blog, VersionOptions.Published).Where(
                p => p.Slug.StartsWith(post.Slug, StringComparison.OrdinalIgnoreCase) &&
                p.Id != post.Id).Select(p => p.Slug);

            //todo: (heskew) need better messages
            if (slugsLikeThis.Count() > 0) {
                //todo: (heskew) need better messages
                _orchardServices.Notifier.Warning(T("A different blog post is already published with this same slug."));

                if (post.ContentItem.VersionRecord == null || post.ContentItem.VersionRecord.Published)
                {
                    var originalSlug = post.Slug;
                    //todo: (heskew) make auto-uniqueness optional
                    post.Slug = _routableService.GenerateUniqueSlug(post.Slug, slugsLikeThis);

                    if (originalSlug != post.Slug)
                        _orchardServices.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created blog post so this post now has the slug \"{1}\"",
                                                     originalSlug, post.Slug));
                }
            }
        }
    }
}
