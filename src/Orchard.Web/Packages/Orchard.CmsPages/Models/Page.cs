using System.Collections.Generic;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Orchard.Data.Conventions;

namespace Orchard.CmsPages.Models {

    public class Page {
        public Page() {
            Revisions = new List<PageRevision>();
            Scheduled = new List<Scheduled>();
        }
        
        public virtual int Id { get; protected set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<PageRevision> Revisions { get; protected set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<Scheduled> Scheduled { get; protected set; }
        
        public virtual Published Published { get; set; }
    }

    public class PageOverride : IAutoMappingOverride<Page> {
        public void Override(AutoMapping<Page> mapping) {
            mapping.HasOne(p => p.Published).PropertyRef(p => p.Page).Cascade.All();
        }
    }
}
