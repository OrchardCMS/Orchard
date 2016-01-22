using System;
using System.Linq;
using Orchard.ContentPicker.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentPicker.ViewModels;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.ContentPicker.Drivers {
    public class ContentPickerFieldDriver : ContentFieldDriver<Fields.ContentPickerField> {
        private readonly IContentManager _contentManager;

        public ContentPickerFieldDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(Fields.ContentPickerField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(Fields.ContentPickerField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.ContentPickerField field, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Fields_ContentPicker", GetDifferentiator(field, part), () => shapeHelper.Fields_ContentPicker()),
                ContentShape("Fields_ContentPicker_SummaryAdmin", GetDifferentiator(field, part), () => {
                    var unpublishedIds = field.Ids.Except(field.ContentItems.Select(x => x.Id));
                    var unpublishedContentItems = _contentManager.GetMany<ContentItem>(unpublishedIds, VersionOptions.Latest, QueryHints.Empty).ToList();

                    return shapeHelper.Fields_ContentPicker_SummaryAdmin(UnpublishedContentItems: unpublishedContentItems);
                }));
        }

        protected override DriverResult Editor(ContentPart part, Fields.ContentPickerField field, dynamic shapeHelper) {
            return ContentShape("Fields_ContentPicker_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new ContentPickerFieldViewModel {
                        Field = field,
                        Part = part,
                        ContentItems = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Latest, QueryHints.Empty).ToList()
                    };

                    model.SelectedIds = string.Join(",", field.Ids);

                    return shapeHelper.EditorTemplate(TemplateName: "Fields/ContentPicker.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, Fields.ContentPickerField field, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ContentPickerFieldViewModel { SelectedIds = string.Join(",", field.Ids) };

            updater.TryUpdateModel(model, GetPrefix(field, part), null, null);

            var settings = field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldSettings>();

            if (String.IsNullOrEmpty(model.SelectedIds)) {
                field.Ids = new int[0];
            }
            else {
                field.Ids = model.SelectedIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            }

            if (settings.Required && field.Ids.Length == 0) {
                updater.AddModelError("Id", T("The field {0} is mandatory", field.Name.CamelFriendly()));
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, Fields.ContentPickerField field, ImportContentContext context) {
            var contentItemIds = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "ContentItems");
            if (contentItemIds != null) {
                field.Ids = contentItemIds.Split(',')
                    .Select(context.GetItemFromSession)
                    .Select(contentItem => contentItem.Id).ToArray();
            }
            else {
                field.Ids = new int[0];
            }
        }

        protected override void Exporting(ContentPart part, Fields.ContentPickerField field, ExportContentContext context) {
            if (field.Ids.Any()) {
                var contentItemIds = field.Ids
                    .Select(x => _contentManager.Get(x))
                    .Where(x => x != null)
                    .Select(x => _contentManager.GetItemMetadata(x).Identity.ToString())
                    .ToArray();

                context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("ContentItems", string.Join(",", contentItemIds));
            }
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Ids"), T("A formatted list of the ids, e.g., {1},{42}"));
        }
    }
}