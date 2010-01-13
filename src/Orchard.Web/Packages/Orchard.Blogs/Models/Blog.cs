using System.Web.Mvc;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Models {
    public class Blog : ContentPart<BlogRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        public string Name {
            get { return this.As<RoutableAspect>().Title; }
        }

        //TODO: (erikpo) Need a data type for slug
        public string Slug {
            get { return this.As<RoutableAspect>().Slug; }
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