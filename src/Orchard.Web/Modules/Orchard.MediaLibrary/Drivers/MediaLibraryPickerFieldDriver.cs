using System;
using System.Linq;
using Orchard.MediaLibrary.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.MediaLibrary.Drivers {
    public class MediaLibraryPickerFieldDriver : ContentFieldDriver<Fields.MediaLibraryPickerField> {
        private readonly IContentManager _contentManager;

        public MediaLibraryPickerFieldDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(Fields.MediaLibraryPickerField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(Fields.MediaLibraryPickerField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.MediaLibraryPickerField field, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Fields_MediaLibraryPicker", GetDifferentiator(field, part), () => shapeHelper.Fields_MediaLibraryPicker()),
                ContentShape("Fields_MediaLibraryPicker_Summary", GetDifferentiator(field, part), () => shapeHelper.Fields_MediaLibraryPicker_Summary()),
                ContentShape("Fields_MediaLibraryPicker_SummaryAdmin", GetDifferentiator(field, part), () => shapeHelper.Fields_MediaLibraryPicker_SummaryAdmin())
            );
        }

        protected override DriverResult Editor(ContentPart part, Fields.MediaLibraryPickerField field, dynamic shapeHelper) {
            return ContentShape("Fields_MediaLibraryPicker_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new MediaLibraryPickerFieldViewModel {
                        Field = field,
                        Part = part,
                        ContentItems = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Published, QueryHints.Empty).ToList(),
                    };

                    model.SelectedIds = string.Concat(",", field.Ids);

                    return shapeHelper.EditorTemplate(TemplateName: "Fields/MediaLibraryPicker.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, Fields.MediaLibraryPickerField field, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MediaLibraryPickerFieldViewModel { SelectedIds = string.Join(",", field.Ids) };

            updater.TryUpdateModel(model, GetPrefix(field, part), null, null);

            var settings = field.PartFieldDefinition.Settings.GetModel<MediaLibraryPickerFieldSettings>();

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

        protected override void Importing(ContentPart part, Fields.MediaLibraryPickerField field, ImportContentContext context) {
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

        protected override void Exporting(ContentPart part, Fields.MediaLibraryPickerField field, ExportContentContext context) {
            if (field.Ids.Any()) {
                var contentItemIds = field.Ids
                    .Select(x => _contentManager.Get(x))
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