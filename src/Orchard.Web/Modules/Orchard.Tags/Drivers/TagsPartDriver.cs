using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Tags.Helpers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;

namespace Orchard.Tags.Drivers {
    [UsedImplicitly]
    public class TagsPartDriver : ContentPartDriver<TagsPart> {
        private const string TemplateName = "Parts/Tags";
        private readonly ITagService _tagService;

        public TagsPartDriver(ITagService tagService) {
            _tagService = tagService;
        }

        protected override string Prefix {
            get { return "Tags"; }
        }

        protected override DriverResult Display(TagsPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Tags_ShowTags",
                            () => shapeHelper.Parts_Tags_ShowTags(ContentPart: part, Tags: part.CurrentTags));
        }

        protected override DriverResult Editor(TagsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Tags_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: BuildEditorViewModel(part), Prefix: Prefix));
        }

        protected override DriverResult Editor(TagsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new EditTagsViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            var tagNames = TagHelpers.ParseCommaSeparatedTagNames(model.Tags);
            if (part.ContentItem.Id != 0) {
                _tagService.UpdateTagsForContentItem(part.ContentItem, tagNames);
            }

            return ContentShape("Parts_Tags_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        private static EditTagsViewModel BuildEditorViewModel(TagsPart part) {
            return new EditTagsViewModel {
                Tags = string.Join(", ", part.CurrentTags.Select((t, i) => t.TagName).ToArray())
            };
        }

        protected override void Importing(TagsPart part, ImportContentContext context) {
            var tagString = context.Attribute(part.PartDefinition.Name, "Tags");
            if (tagString != null) {
                var tags = tagString.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                // Merge tags.
                if (tags.Length > 0) {
                    var currentTags = part.CurrentTags.Select(t => t.TagName);
                    _tagService.UpdateTagsForContentItem(context.ContentItem, tags.Concat(currentTags).Distinct()); 
                }
            }
        }

        protected override void Exporting(TagsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Tags", String.Join(",", part.CurrentTags.Select(t => t.TagName)));
        }
    }
}