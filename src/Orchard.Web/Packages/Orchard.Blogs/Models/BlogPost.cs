using System;
using System.Web.Routing;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPart<BlogPostRecord>, IContentDisplayInfo {
        public readonly static ContentType ContentType = new ContentType { Name = "blogpost", DisplayName = "Blog Post" };

        public Blog Blog { get; set; }
        public int Id { get { return ContentItem.Id; } }
        public string Title { get { return this.As<RoutableAspect>().Title; } }
        public string Body { get { return this.As<BodyAspect>().Record.Text; } }
        public string Slug { get { return this.As<RoutableAspect>().Slug; } }
        public IUser Creator { get { return this.As<CommonAspect>().OwnerField.Value; } }
        public DateTime? Published { get { return Record.Published; } }

        #region IContentDisplayInfo Members

        public string DisplayText {
            get { return Title; }
        }

        public RouteValueDictionary DisplayRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Blogs", controller = "BlogPost", action = "Item", blogSlug = Blog.Slug, postSlug = Slug });
        }

        public RouteValueDictionary EditRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Blogs", controller = "BlogPost", action = "Edit", blogSlug = Blog.Slug, postSlug = Slug });
        }

        #endregion
    }
}