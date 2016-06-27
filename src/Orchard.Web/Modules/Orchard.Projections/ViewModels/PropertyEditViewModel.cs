using System.ComponentModel.DataAnnotations;
using Orchard.Projections.Descriptors.Property;

namespace Orchard.Projections.ViewModels {

    public class PropertyEditViewModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public PropertyDescriptor Property { get; set; }
        public dynamic Form { get; set; }

        public bool ExcludeFromDisplay { get; set; }
        public bool LinkToContent { get; set; }

        // Label
        public bool CreateLabel { get; set; }
        [StringLength(255)]
        public string Label { get; set; }

        // Settings
        public bool CustomizePropertyHtml { get; set; }
        [StringLength(64)]
        public string CustomPropertyTag { get; set; }
        [StringLength(64)]
        public string CustomPropertyCss { get; set; }

        public bool CustomizeLabelHtml { get; set; }
        [StringLength(64)]
        public string CustomLabelTag { get; set; }
        [StringLength(64)]
        public string CustomLabelCss { get; set; }

        public bool CustomizeWrapperHtml { get; set; }
        [StringLength(64)]
        public string CustomWrapperTag { get; set; }
        [StringLength(64)]
        public string CustomWrapperCss { get; set; }

        // No Result
        [StringLength(255)]
        public string NoResultText { get; set; }
        public bool ZeroIsEmpty { get; set; }
        public bool HideEmpty { get; set; }

        public bool RewriteOutput { get; set; }
        [StringLength(255)]
        public string RewriteText { get; set; }
        public bool StripHtmlTags { get; set; }
        public bool TrimLength { get; set; }
        public bool AddEllipsis { get; set; }
        public int MaxLength { get; set; }
        public bool TrimOnWordBoundary { get; set; }
        public bool PreserveLines { get; set; }
        public bool TrimWhiteSpace { get; set; }
    }
}
