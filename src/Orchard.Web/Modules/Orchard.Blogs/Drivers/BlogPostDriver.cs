using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Services;
using Orchard.Localization;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]                                                                                                                                                                                        
    public class BlogPostDriver : ContentItemDriver<BlogPost> {
        public IOrchardServices Services { get; set; }
        private readonly IBlogPostService _blogPostService;
        private readonly IRoutableService _routableService;

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "blogpost",
                                                                             DisplayName = "Blog Post"
                                                                         };

        public BlogPostDriver(IOrchardServices services, IBlogService blogService, IBlogPostService blogPostService, IRoutableService routableService) {
            Services = services;
            _blogPostService = blogPostService;
            _routableService = routableService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(BlogPost post) {
            return post.Title;
        }

        public override RouteValueDictionary GetDisplayRouteValues(BlogPost post) {
            if (post.Blog == null)
                return new RouteValueDictionary();

            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPost"},
                                                {"Action", "Item"},
                                                {"blogSlug", post.Blog.Slug},
                                                {"postSlug", post.Slug},
                                            };
        }

        public override RouteValueDictionary GetEditorRouteValues(BlogPost post) {
            if (post.Blog == null)
                return new RouteValueDictionary();

            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPostAdmin"},
                                                {"Action", "Edit"},
                                                {"blogSlug", post.Blog.Slug},
                                                {"postId", post.Id},
                                            };
        }

        public override RouteValueDictionary GetCreateRouteValues(BlogPost post) {
            if (post.Blog == null)
                return new RouteValueDictionary();

            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPostAdmin"},
                                                {"Action", "Create"},
                                                {"blogSlug", post.Blog.Slug},
                                            };
        }

        protected override DriverResult Display(BlogPost post, string displayType) {
            return Combined(
                ContentItemTemplate("Items/Blogs.BlogPost").LongestMatch(displayType, "Summary", "SummaryAdmin"),
                ContentPartTemplate(post, "Parts/Blogs.BlogPost.Metadata").LongestMatch(displayType, "Summary", "SummaryAdmin").Location("meta", "1"));
        }

        protected override DriverResult Editor(BlogPost post) {
            return Combined(
                ContentItemTemplate("Items/Blogs.BlogPost"),
                ContentPartTemplate(post, "Parts/Blogs.BlogPost.Publish").Location("secondary", "1"));
        }

        protected override DriverResult Editor(BlogPost post, IUpdateModel updater) {
            updater.TryUpdateModel(post, Prefix, null, null);

            DateTime scheduled;
            if (DateTime.TryParse(string.Format("{0} {1}", post.ScheduledPublishUtcDate, post.ScheduledPublishUtcTime), out scheduled))
                post.ScheduledPublishUtc = scheduled;

            return Editor(post);
        }
    }
}