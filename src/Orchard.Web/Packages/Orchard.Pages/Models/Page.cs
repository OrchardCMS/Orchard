using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Security;

namespace Orchard.Pages.Models {
    public class Page : ContentPart<PageRecord> {
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

        public IUser Creator {
            get { return this.As<CommonAspect>().Owner; }
            set { this.As<CommonAspect>().Owner = value; }
        }

        public DateTime? Published {
            get { return Record.Published; }
            set { Record.Published = value; }
        }

        //public Page() {
        //    Revisions = new List<PageRevision>();
        //    Scheduled = new List<Scheduled>();
        //}
        //[CascadeAllDeleteOrphan]
        //public virtual IList<PageRevision> Revisions { get; protected set; }

        //[CascadeAllDeleteOrphan]
        //public virtual IList<Scheduled> Scheduled { get; protected set; }

        //public virtual Published Published { get; set; }
    }
    //public class PageOverride : IAutoMappingOverride<Page> {
    //    public void Override(AutoMapping<Page> mapping) {
    //        mapping.HasOne(p => p.Published).PropertyRef(p => p.Page).Cascade.All();
    //    }
    //}
}
