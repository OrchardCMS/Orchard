using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Blogs.Controllers {
    [UsedImplicitly]
    public class BlogPostDriver : ItemDriver<BlogPost> {
        public BlogPostDriver()
            : base(BlogPost.ContentType) {
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

        protected override DriverResult Editor(BlogPost post) {
            return Combined(
                PartTemplate(post, "Parts/Blogs.BlogPost.Fields").Location("primary", "1"),
                PartTemplate(post, "Parts/Blogs.BlogPost.Publish").Location("secondary", "1")
                );
        }

        protected override DriverResult Editor(BlogPost post, IUpdateModel updater) {
            updater.TryUpdateModel(post, Prefix, null, null);
            return Editor(post);
        }
    }
}
