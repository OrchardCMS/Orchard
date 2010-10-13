using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace Orchard.Blogs.Models {
    public class BlogPart : ContentPart<BlogPartRecord> {
        public string Name {
            get { return this.As<RoutePart>().Title; }
            set { this.As<RoutePart>().Title = value; }
        }

        //TODO: (erikpo) Need a data type for slug
        public string Slug {
            get { return this.As<RoutePart>().Slug; }
            set { this.As<RoutePart>().Slug = value; }
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