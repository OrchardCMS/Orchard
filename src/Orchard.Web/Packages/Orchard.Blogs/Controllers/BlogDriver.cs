using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Blogs.Controllers {
    [UsedImplicitly]
    public class BlogDriver : ItemDriver<Blog> {
        private readonly IContentManager _contentManager;
        private readonly IBlogPostService _blogPostService;

        public BlogDriver(IContentManager contentManager, IBlogPostService blogPostService)
            : base(Blog.ContentType) {
            _contentManager = contentManager;
            _blogPostService = blogPostService;
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

            IEnumerable<ItemDisplayModel<BlogPost>> blogPosts = null;
            if (displayType.StartsWith("DetailAdmin")) {
                blogPosts = _blogPostService.Get(blog)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "SummaryAdmin"));
            }
            else if (displayType.StartsWith("Detail")) {
                blogPosts = _blogPostService.Get(blog)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "Summary"));
            }

            return Combined(
                ItemTemplate("Items/Blogs.Blog").LongestMatch(displayType, "Summary", "DetailAdmin", "SummaryAdmin"),
                blogPosts == null ? null : PartTemplate(blogPosts, "Parts/Blogs.BlogPost.List", "").Location("body"));
        }

        protected override DriverResult Editor(Blog blog) {
            return Combined(
                ItemTemplate("Items/Blogs.Blog"),
                PartTemplate(blog, "Parts/Blogs.Blog.Fields").Location("primary", "1"));
        }

        protected override DriverResult Editor(Blog blog, IUpdateModel updater) {
            updater.TryUpdateModel(blog, Prefix, null, null);
            return Combined(
                ItemTemplate("Items/Blogs.Blog"),
                PartTemplate(blog, "Parts/Blogs.Blog.Fields").Location("primary", "1"));
        }
    }
}
