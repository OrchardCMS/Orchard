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
        public string Title { get { return this.As<RoutableAspect>().Title; } }

        [Required]
        public string Slug { get { return this.As<RoutableAspect>().Slug; } }

        [Required]
        public string Body { get { return this.As<BodyAspect>().Record.Text; } }

        public IUser Creator { get { return this.As<CommonAspect>().OwnerField.Value; } }

        public DateTime? Published
        {
            get { return Record.Published; }
            set { Record.Published = value; }
        }

    }
}