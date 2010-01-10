using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPart<BlogPostRecord> {
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
            get { return this.As<ICommonAspect>().Container.As<Blog>(); }
            set { this.As<ICommonAspect>().Container = value; }
        }

        public IUser Creator {
            get { return this.As<ICommonAspect>().Owner; }
            set { this.As<ICommonAspect>().Owner = value; }
        }

        public DateTime? Published
        {
            get { return Record.Published; }
            set { Record.Published = value; }
        }
    }
}