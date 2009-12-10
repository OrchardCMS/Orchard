using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPart<BlogPostRecord> {
        public readonly static ContentType ContentType = new ContentType { Name = "blogpost", DisplayName = "Blog Post" };

        public Blog Blog { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        [Required]
        public string Title {
            get { return this.As<RoutableAspect>().Title; }
            set { this.As<RoutableAspect>().Record.Title = value; }
        }

        [Required]
        public string Slug {
            get { return this.As<RoutableAspect>().Slug; }
            set { this.As<RoutableAspect>().Record.Slug = value; }
        }

        [Required]
        public string Body {
            get { return this.As<BodyAspect>().Record.Text; }
            set { this.As<BodyAspect>().Record.Text = value; }
        }

        public IUser Creator {
            get { return this.As<CommonAspect>().OwnerField.Value; }
            set { this.As<CommonAspect>().Record.OwnerId = value.Id; }
        }

        public DateTime? Published
        {
            get { return Record.Published; }
            set { Record.Published = value; }
        }
    }
}