using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]                                                                                                                                                                                        
    public class BlogPostDriver : ContentItemDriver<BlogPostPart> {
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

        protected override string GetDisplayText(BlogPostPart postPart) {
            return postPart.Title;
        }

        public override RouteValueDictionary GetDisplayRouteValues(BlogPostPart postPart) {
            if (postPart.BlogPart == null)
                return new RouteValueDictionary();

            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPost"},
                                                {"Action", "Item"},
                                                {"blogSlug", postPart.BlogPart.Slug},
                                                {"postSlug", postPart.Slug},
                                            };
        }

        public override RouteValueDictionary GetEditorRouteValues(BlogPostPart postPart) {
            if (postPart.BlogPart == null)
                return new RouteValueDictionary();

            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPostAdmin"},
                                                {"Action", "Edit"},
                                                {"blogSlug", postPart.BlogPart.Slug},
                                                {"postId", postPart.Id},
                                            };
        }

        public override RouteValueDictionary GetCreateRouteValues(BlogPostPart postPart) {
            if (postPart.BlogPart == null)
                return new RouteValueDictionary();

            return new RouteValueDictionary {
                                                {"Area", "Orchard.Blogs"},
                                                {"Controller", "BlogPostAdmin"},
                                                {"Action", "Create"},
                                                {"blogSlug", postPart.BlogPart.Slug},
                                            };
        }

        protected override DriverResult Editor(BlogPostPart postPart) {
            return ContentItemTemplate("Items/Blogs.BlogPost");
        }

        protected override DriverResult Editor(BlogPostPart postPart, IUpdateModel updater) {
            return Editor(postPart);
        }
    }
}