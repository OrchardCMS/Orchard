using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Projections.Models {
    public class ProjectionPartRecord : ContentPartRecord {
        public ProjectionPartRecord() {
            MaxItems = 20;
        }

        /// <summary>
        /// Maximum number of items to retrieve from db
        /// </summary>
        public virtual int Items { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public virtual int ItemsPerPage { get; set; }

        /// <summary>
        /// Number of items to skip
        /// </summary>
        public virtual int Skip { get; set; }

        /// <summary>
        /// Suffix to use when multiple pagers are available on the same page
        /// </summary>
        [StringLength(255)]
        public virtual string PagerSuffix { get; set; }

        /// <summary>
        /// The maximum number of items which can be requested at once. 
        /// </summary>
        public virtual int MaxItems { get; set; }

        /// <summary>
        /// True to render a pager
        /// </summary>
        public virtual bool DisplayPager { get; set; }

        /// <summary>
        /// The query to execute
        /// </summary>
        [Aggregate]
        public virtual QueryPartRecord QueryPartRecord { get; set; }

        /// <summary>
        /// The layout to render
        /// </summary>
        [Aggregate]
        public virtual LayoutRecord LayoutRecord { get; set; }

    }
}