using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

namespace Orchard.Rules.Providers {
    public class ContentEvents : IEventProvider {
        public ContentEvents() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeEventContext describe) {
            Func<dynamic, bool> contentHasPart = ContentHasPart;

            describe.For("Content", T("Content Items"), T("Content Items"))
                .Element("Created", T("Content Created"), T("Content is actually created."), contentHasPart, context => T("When content with types ({0}) is created.", FormatPartsList(context)), "SelectContentTypes")
                .Element("Versioned", T("Content Versioned"), T("Content is actually versioned."), contentHasPart, context => T("When content with types ({0}) is versioned.", FormatPartsList(context)), "SelectContentTypes")
                .Element("Published", T("Content Published"), T("Content is actually published."), contentHasPart, context => T("When content with types ({0}) is published.", FormatPartsList(context)), "SelectContentTypes")
                .Element("Removed", T("Content Removed"), T("Content is actually removed."), contentHasPart, context => T("When content with types ({0}) is removed.", FormatPartsList(context)), "SelectContentTypes");
        }

        private string FormatPartsList(EventContext context) {
            var contenttypes = context.Properties.ContainsKey("ContentTypes") ? context.Properties["ContentTypes"] : context.Properties["contenttypes"];

            if (String.IsNullOrEmpty(contenttypes)) {
                return T("Any").Text;
            }

            return contenttypes;
        }

        private static bool ContentHasPart(dynamic context) {
            string contenttypes = context.Properties["ContentTypes"];
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