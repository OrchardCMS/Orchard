using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.Blogs.Models {
    [OrchardFeature("Blog")]
    public class Blog : ContentPart<BlogRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        public string Name {
            get { return this.As<RoutableAspect>().Title; }
        }

        //TODO: (erikpo) Need a data type for slug
        public string Slug {
            get { return this.As<RoutableAspect>().Slug; }
            set { this.As<RoutableAspect>().Slug = value; }
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