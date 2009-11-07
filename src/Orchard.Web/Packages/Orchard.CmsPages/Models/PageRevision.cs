using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.Data.Conventions;

namespace Orchard.CmsPages.Models {

    public class PageRevision {
        public PageRevision() {
            Contents = new List<ContentItem>();
            Scheduled = new List<Scheduled>();
        }

        public virtual int Id { get; set; }

        public virtual Page Page { get; set; }

        public virtual int Number { get; set; }

        [Required, DisplayName("Permalink:")]
        public virtual string Slug { get; set; }

        [Required, DisplayName("Title:")]
        public virtual string Title { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<ContentItem> Contents { get; protected set; }

        [DisplayName("Template:")]
        public virtual string TemplateName { get; set; }

        public virtual DateTime? PublishedDate { get; set; }

        public virtual DateTime? ModifiedDate { get; set; }

        public virtual IList<Scheduled> Scheduled { get; protected set; }
    }
}