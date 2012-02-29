using System;
using System.Linq;
using JetBrains.Annotations;
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
    [UsedImplicitly]
    public class TagsPartDriver : ContentPartDriver<TagsPart> {
        private static readonly char[] _disalowedChars = new [] { '<', '>', '*', '%', ':', '&', '\\', '"', '|' };
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
                            () => shapeHelper.Parts_Tags_ShowTags(Tags: part.CurrentTags));
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

            var disallowedTags = tagNames.Where(x => _disalowedChars.Intersect(x).Any()).ToList();

            if (disallowedTags.Any()) {
                _notifier.Warning(T("The tags \"{0}\" could not be added because they contain forbidden chars: {1}", String.Join(", ", disallowedTags), String.Join(", ", _disalowedChars)));
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