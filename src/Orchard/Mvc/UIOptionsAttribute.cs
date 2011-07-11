using System;
using System.Web.Mvc;

namespace Orchard.Mvc {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
// ReSharper disable InconsistentNaming
    public class UIOptionsAttribute : Attribute, IMetadataAware {
// ReSharper restore InconsistentNaming
        public string Position { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
        public string Template { get; set; }

        public string EnabledBy { get; set; }
        public bool EnableWrapper { get; set; }

        public string ActionLink { get; set; }
        public string ActionLinkController { get; set; }
        public string ActionLinkArea { get; set; }
        public string ActionLinkText { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata) {
            if (DisplayName != null) {
                metadata.DisplayName = DisplayName;
            }
            if (Template != null) {
                metadata.TemplateHint = Template;
            }
            if (Description != null) {
                metadata.Description = Description;
            }
            metadata.AdditionalValues["Position"] = Position;
            metadata.AdditionalValues["EnableWrapper"] = EnableWrapper;
            metadata.AdditionalValues["EnabledBy"] = EnabledBy;
            metadata.AdditionalValues["ActionLink"] = ActionLink;
            metadata.AdditionalValues["ActionLinkController"] = ActionLinkController;
            metadata.AdditionalValues["ActionLinkArea"] = ActionLinkArea;
            metadata.AdditionalValues["ActionLinkText"] = ActionLinkText;
            metadata.AdditionalValues["ErrorMessage"] = ErrorMessage;
        }
    }
}
