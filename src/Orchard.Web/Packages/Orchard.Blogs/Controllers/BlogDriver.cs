using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.Controllers {
    [UsedImplicitly]
    public class BlogDriver : ContentItemDriver<Blog> {
        public readonly static ContentType ContentType = new ContentType {
            Name = "blog",
            DisplayName = "Blog"
        };

        private readonly IContentManager _contentManager;
        private readonly IBlogPostService _blogPostService;

        public BlogDriver(IContentManager contentManager, IBlogPostService blogPostService) {
            _contentManager = contentManager;
            _blogPostService = blogPostService;
        }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(Blog item) {
            return item.Name;
        }

        protected override RouteValueDictionary GetDisplayRouteValues(Blog blog) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "Blog"},
                                                {"Action", "Item"},
                                                {"blogSlug", blog.Slug}
                                            };
        }

        protected override RouteValueDictionary GetEditorRouteValues(Blog blog) {
            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "Blog"},
                                                {"Action", "Edit"},
                                                {"blogSlug", blog.Slug}
                                            };
        }

        protected override DriverResult Display(Blog blog, string displayType) {

            IEnumerable<ContentItemViewModel<BlogPost>> blogPosts = null;
            if (displayType.StartsWith("DetailAdmin")) {
                blogPosts = _blogPostService.Get(blog, VersionOptions.Latest)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "SummaryAdmin"));
            }
            else if (displayType.StartsWith("Detail")) {
                blogPosts = _blogPostService.Get(blog)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "Summary"));
            }

            return Combined(
                ContentItemTemplate("Items/Blogs.Blog").LongestMatch(displayType, "Summary", "DetailAdmin", "SummaryAdmin"),
                blogPosts == null ? null : ContentPartTemplate(blogPosts, "Parts/Blogs.BlogPost.List", "").Location("primary"));
        }

        protected override DriverResult Editor(Blog blog) {
            return Combined(
                ContentItemTemplate("Items/Blogs.Blog"),
                ContentPartTemplate(blog, "Parts/Blogs.Blog.Fields").Location("primary", "1"));
        }

        protected override DriverResult Editor(Blog blog, IUpdateModel updater) {
            updater.TryUpdateModel(blog, Prefix, null, null);
            return Combined(
                ContentItemTemplate("Items/Blogs.Blog"),
                ContentPartTemplate(blog, "Parts/Blogs.Blog.Fields").Location("primary", "1"));
        }
    }
}
