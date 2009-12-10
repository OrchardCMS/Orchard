using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPart<BlogPostRecord> {
        public readonly static ContentType ContentType = new ContentType { Name = "blogpost", DisplayName = "Blog Post" };

        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        [Required]
        public string Title {
            get { return this.As<RoutableAspect>().Title; }
            set { this.As<RoutableAspect>().Title = value; }
        }

        [Required]
        public string Slug {
            get { return this.As<RoutableAspect>().Slug; }
            set { this.As<RoutableAspect>().Slug = value; }
        }

        public Blog Blog {
            get { return this.As<CommonAspect>().Container.As<Blog>(); }
            set { this.As<CommonAspect>().Container = value; }
        }

        public IUser Creator {
            get { return this.As<CommonAspect>().Owner; }
            set { this.As<CommonAspect>().Owner = value; }
        }

        public DateTime? Published
        {
            get { return Record.Published; }
            set { Record.Published = value; }
        }
    }
}