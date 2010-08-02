using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Contents.ViewModels;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Feeds;
using Orchard.Localization;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPartDriver : ContentItemDriver<BlogPart> {
        public IOrchardServices Services { get; set; }

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "Blog",
                                                                             DisplayName = "Blog"
                                                                         };

        private readonly IContentManager _contentManager;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;

        public BlogPartDriver(IOrchardServices services, IContentManager contentManager, IBlogPostService blogPostService, IFeedManager feedManager) {
            Services = services;
            _contentManager = contentManager;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(BlogPart item) {
            return item.Name;
        }

        public override RouteValueDictionary GetDisplayRouteValues(BlogPart blogPart) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "Blog"},
                                                {"Action", "Item"},
                                                {"blogSlug", blogPart.Slug}
                                            };
        }

        public override RouteValueDictionary GetEditorRouteValues(BlogPart blogPart) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "Blog"},
                                                {"Action", "Edit"},
                                                {"blogSlug", blogPart.Slug}
                                            };
        }

        protected override DriverResult Display(BlogPart blogPart, string displayType) {

            IEnumerable<ContentItemViewModel<BlogPostPart>> blogPosts = null;
            if (displayType.StartsWith("DetailAdmin")) {
                blogPosts = _blogPostService.Get(blogPart, VersionOptions.Latest)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "SummaryAdmin"));
            }
            else if (displayType.StartsWith("Detail")) {
                blogPosts = _blogPostService.Get(blogPart)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "Summary"));
                _feedManager.Register(blogPart);
            }

            return Combined(
                ContentItemTemplate("Items/Blogs.Blog").LongestMatch(displayType, "Summary", "DetailAdmin", "SummaryAdmin"),
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Manage").Location("manage"),
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Metadata").Location("metadata"),
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Description").Location("manage", "after"),
                blogPosts == null
                    ? null
                    : ContentPartTemplate(
                        new ListContentsViewModel {
                            ContainerId = blogPart.Id,
                            Entries = blogPosts.Select(bp => new ListContentsViewModel.Entry {
                                ContentItem = bp.Item.ContentItem,
                                ContentItemMetadata = _contentManager.GetItemMetadata(bp.Item.ContentItem),
                                ViewModel = bp
                            }).ToList()
                        },
                        "Parts/Blogs.BlogPost.List",
                        "").LongestMatch(displayType, "DetailAdmin").Location("primary"));
        }

        protected override DriverResult Editor(BlogPart blogPart) {
            var location = blogPart.GetLocation("Editor");
            return Combined(
                ContentItemTemplate("Items/Blogs.Blog"),
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Fields").Location(location));
        }

        protected override DriverResult Editor(BlogPart blogPart, IUpdateModel updater) {
            updater.TryUpdateModel(blogPart, Prefix, null, null);
            return Editor(blogPart);
        }
    }
}