using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]                                                                                                                                                                                        
    public class BlogPostDriver : ContentItemDriver<BlogPost> {
        public IOrchardServices Services { get; set; }

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "BlogPost",
                                                                             DisplayName = "Blog Post"
                                                                         };

        public BlogPostDriver(IOrchardServices services) {
            Services = services;
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

        protected override DriverResult Editor(BlogPost post) {
            return ContentItemTemplate("Items/Blogs.BlogPost");
        }

        protected override DriverResult Editor(BlogPost post, IUpdateModel updater) {
            return Editor(post);
        }
    }
}