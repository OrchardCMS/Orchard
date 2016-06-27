using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Tags.Helpers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Tags.Drivers {
    public class TagsPartDriver : ContentPartDriver<TagsPart> {
        public static readonly char[] DisalowedChars =  { '<', '>', '*', '%', ':', '&', '\\', '"', '|', '/' };
        private const string TemplateName = "Parts/Tags";
        private readonly ITagService _tagService;
        private readonly INotifier _notifier;

        public TagsPartDriver(ITagService tagService, INotifier notifier) {
            _tagService = tagService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Tags"; }
        }

        protected override DriverResult Display(TagsPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Tags_ShowTags",
                            () => shapeHelper.Parts_Tags_ShowTags(Tags: part.CurrentTags.Select(x => new ShowTagViewModel { TagName = x })));
        }

        protected override DriverResult Editor(TagsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Tags_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: BuildEditorViewModel(part), Prefix: Prefix));
        }

        protected override DriverResult Editor(TagsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new EditTagsViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            var tagNames = TagHelpers.ParseCommaSeparatedTagNames(model.Tags);

            // as the tag names are used in the route directly, prevent them from having ASP.NET disallowed chars
            // c.f., http://www.hanselman.com/blog/ExperimentsInWackinessAllowingPercentsAnglebracketsAndOtherNaughtyThingsInTheASPNETIISRequestURL.aspx

            var disallowedTags = tagNames.Where(x => DisalowedChars.Intersect(x).Any()).ToList();

            if (disallowedTags.Any()) {
                _notifier.Warning(T("The tags \"{0}\" could not be added because they contain forbidden chars: {1}", String.Join(", ", disallowedTags), String.Join(", ", DisalowedChars)));
                tagNames = tagNames.Where(x => !disallowedTags.Contains(x)).ToList();
            }

            if (part.ContentItem.Id != 0) {
                _tagService.UpdateTagsForContentItem(part.ContentItem, tagNames);
            }

            return ContentShape("Parts_Tags_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        private static EditTagsViewModel BuildEditorViewModel(TagsPart part) {
            return new EditTagsViewModel {
                Tags = string.Join(", ", part.CurrentTags)
            };
        }

        protected override void Importing(TagsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            var tagString = context.Attribute(part.PartDefinition.Name, "Tags");
            if (tagString != null) {
                var tags = tagString.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                // Merge tags.
                if (tags.Length > 0) {
                    var currentTags = part.CurrentTags;
                    _tagService.UpdateTagsForContentItem(context.ContentItem, tags.Concat(currentTags).Distinct()); 
                }
            }
        }

        protected override void Exporting(TagsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Tags", String.Join(",", part.CurrentTags));
        }
    }
}