using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.Models;
using Orchard.Models;

namespace Orchard.Blogs.Models {
    public class Blog : ContentPart<BlogRecord> {
        public readonly static ContentType ContentType = new ContentType { Name = "blog", DisplayName = "Blog" };

        public int Id { get { return ContentItem.Id; } }

        [Required]
        public string Name {
            get { return this.As<RoutableAspect>().Title; }
            set { this.As<RoutableAspect>().Record.Title = value; }
        }

        //TODO: (erikpo) Need a data type for slug
        [Required]
        public string Slug {
            get { return this.As<RoutableAspect>().Slug; }
            set { this.As<RoutableAspect>().Record.Slug = value; }
        }

        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }
        
        //public bool Enabled { get { return Record.Enabled; } }
        
        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }
    }
}