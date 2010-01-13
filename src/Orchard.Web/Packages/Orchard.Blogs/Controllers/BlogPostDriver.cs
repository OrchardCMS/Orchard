using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Blogs.Controllers {
    [UsedImplicitly]
    public class BlogPostDriver : ContentItemDriver<BlogPost> {
        public readonly static ContentType ContentType = new ContentType {
            Name = "blogpost",
            DisplayName = "Blog Post"
        };

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
            return Editor(post);
        }
    }
}
