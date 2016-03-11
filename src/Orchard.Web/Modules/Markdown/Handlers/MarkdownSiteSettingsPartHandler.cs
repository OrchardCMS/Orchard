using System;
using System.Linq;
using Markdown.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Localization;

namespace Markdown.Handlers {
    public class MarkdownSiteSettingsPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MarkdownSiteSettingsPartHandler(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
            Filters.Add(new ActivatingFilter<MarkdownSiteSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<MarkdownSiteSettingsPart>("MarkdownSiteSettings", "Parts/Markdown.MarkdownSiteSettings", "markdown"));
            OnInitializing<MarkdownSiteSettingsPart>((context, part) => {
                part.UseMarkdownForBlogs = false;
            });

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Markdown")));
        }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            if (!string.Equals("markdown", context.GroupId, StringComparison.OrdinalIgnoreCase))
                return;

            var part = context.ContentItem.As<MarkdownSiteSettingsPart>();
            if (part == null) {
                return;
            }

            base.UpdateEditorShape(context);
            
            var blogPost = _contentDefinitionManager.GetTypeDefinition("BlogPost");
            if (blogPost == null) {
                return;
            }

            var bodyPart = blogPost.Parts.FirstOrDefault(x => x.PartDefinition.Name == "BodyPart");
            if (bodyPart == null) {
                return;
            }

            _contentDefinitionManager.AlterTypeDefinition("BlogPost", build => build
                .WithPart("BodyPart", cfg => cfg
                    .WithSetting("BodyTypePartSettings.Flavor", part.UseMarkdownForBlogs ? "markdown" : "html")
                )
            );
        }
    }
}