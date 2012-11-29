using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Models.Descriptors;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Providers {

    public class ContentActivityProvider : IActivityProvider {
        public ContentActivityProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeActivityContext describe) {
            describe.For("Content", T("Content Items"), T("Content Items"))
                .Element("Created", T("Content Created"), T("Content is actually created."), ContentHasPart, context => T("When content with types ({0}) is created.", FormatPartsList(context)), "SelectContentTypes")
                .Element("Versioned", T("Content Versioned"), T("Content is actually versioned."), ContentHasPart, context => T("When content with types ({0}) is versioned.", FormatPartsList(context)), "SelectContentTypes")
                .Element("Published", T("Content Published"), T("Content is actually published."), ContentHasPart, context => T("When content with types ({0}) is published.", FormatPartsList(context)), "SelectContentTypes")
                .Element("Removed", T("Content Removed"), T("Content is actually removed."), ContentHasPart, context => T("When content with types ({0}) is removed.", FormatPartsList(context)), "SelectContentTypes");
        }

        private string FormatPartsList(ActivityContext context) {
            string contenttypes = context.State.ContentTypes;

            if (String.IsNullOrEmpty(contenttypes)) {
                return T("Any").Text;
            }

            return contenttypes;
        }

        private bool ContentHasPart(ActivityContext context) {
            string contenttypes = context.State.ContentTypes;
            var content = context.Tokens["Content"] as IContent;

            // "" means 'any'
            if (String.IsNullOrEmpty(contenttypes)) {
                return true;
            }

            if (content == null) {
                return false;
            }

            var contentTypes = contenttypes.Split(new[] { ',' });

            return contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
        }
    }
}