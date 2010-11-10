using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Packaging.Models {
    public class PackagingSourceRecord {
        public virtual int Id { get; set; }
        public virtual string FeedTitle { get; set; }
        public virtual string FeedUrl { get; set; }
    }
}