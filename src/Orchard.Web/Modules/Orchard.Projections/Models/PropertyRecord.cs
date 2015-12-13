using System.ComponentModel.DataAnnotations;

namespace Orchard.Projections.Models {
    public class PropertyRecord {
        public virtual int Id { get; set; }
        public virtual string Description { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual string State { get; set; }
        public virtual int Position { get; set; }

        public virtual bool ExcludeFromDisplay { get; set; }
        public virtual bool LinkToContent { get; set; }

        // Label
        public virtual bool CreateLabel { get; set; }
        [StringLength(255)]
        public virtual string Label { get; set; }

        // Styling
        public virtual bool CustomizePropertyHtml { get; set; }
        [StringLength(64)]
        public virtual string CustomPropertyTag { get; set; }
        [StringLength(64)]
        public virtual string CustomPropertyCss { get; set; }

        public virtual bool CustomizeLabelHtml { get; set; }
        [StringLength(64)]
        public virtual string CustomLabelTag { get; set; }
        [StringLength(64)]
        public virtual string CustomLabelCss { get; set; }

        public virtual bool CustomizeWrapperHtml { get; set; }
        [StringLength(64)]
        public virtual string CustomWrapperTag { get; set; }
        [StringLength(64)]
        public virtual string CustomWrapperCss { get; set; }

        // No Result
        public virtual string NoResultText { get; set; }
        public virtual bool ZeroIsEmpty { get; set; }
        public virtual bool HideEmpty { get; set; }

        // Rewrite Result
        public virtual bool RewriteOutput { get; set; }
        public virtual string RewriteText { get; set; }
        public virtual bool StripHtmlTags { get; set; }
        public virtual bool TrimLength { get; set; }
        public virtual bool AddEllipsis { get; set; }
        public virtual int MaxLength { get; set; }
        public virtual bool TrimOnWordBoundary { get; set; }
        public virtual bool PreserveLines { get; set; }
        public virtual bool TrimWhiteSpace { get; set; }

        // Parent property
        public virtual LayoutRecord LayoutRecord { get; set; }
    }
}