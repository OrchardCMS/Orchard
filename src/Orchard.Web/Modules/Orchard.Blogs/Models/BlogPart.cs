using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace Orchard.Blogs.Models {
    public class BlogPart : ContentPart<BlogPartRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        public string Name {
            get { return this.As<IsRoutable>().Title; }
            set { this.As<IsRoutable>().Title = value; }
        }

        //TODO: (erikpo) Need a data type for slug
        public string Slug {
            get { return this.As<IsRoutable>().Slug; }
            set { this.As<IsRoutable>().Slug = value; }
        }

        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }
    }
}