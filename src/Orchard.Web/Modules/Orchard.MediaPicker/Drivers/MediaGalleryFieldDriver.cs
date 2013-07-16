using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.MediaPicker.Fields;
using Orchard.MediaPicker.Settings;
using Orchard.MediaPicker.ViewModels;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.MediaPicker.Drivers {
    public class MediaGalleryFieldDriver : ContentFieldDriver<MediaGalleryField> {
        private readonly IJsonConverter _jsonConverter;

        public MediaGalleryFieldDriver(IJsonConverter jsonConverter) {
            _jsonConverter = jsonConverter;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(MediaGalleryField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(MediaGalleryField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, MediaGalleryField field, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Fields_MediaGallery", GetDifferentiator(field, part), () => shapeHelper.Fields_MediaGallery()),
                ContentShape("Fields_MediaGallery_SummaryAdmin", GetDifferentiator(field, part), () => shapeHelper.Fields_MediaGallery_SummaryAdmin())
            );
        }

        protected override DriverResult Editor(ContentPart part, MediaGalleryField field, dynamic shapeHelper) {
            return ContentShape("Fields_MediaGallery_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new MediaGalleryFieldViewModel {
                        Field = field,
                        Items = field.Items,
                        SelectedItems = field.SelectedItems

                    };

                    model.SelectedItems = string.Concat(",", field.Items);

                    return shapeHelper.EditorTemplate(TemplateName: "Fields/MediaGallery.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, MediaGalleryField field, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MediaGalleryFieldViewModel();

            updater.TryUpdateModel(model, GetPrefix(field, part), null, null);

            var settings = field.PartFieldDefinition.Settings.GetModel<MediaGalleryFieldSettings>();

            if (String.IsNullOrEmpty(model.SelectedItems)) {
                field.SelectedItems = "[]";
            }
            else {
                field.SelectedItems = model.SelectedItems;
            }

            var allItems = _jsonConverter.Deserialize<MediaGalleryItem[]>(field.SelectedItems);

            if (settings.Required && allItems.Length == 0) {
                updater.AddModelError("SelectedItems", T("The field {0} is mandatory", field.Name.CamelFriendly()));
            }

            if (!settings.Multiple && allItems.Length > 1) {
                updater.AddModelError("SelectedItems", T("The field {0} doesn't accept multiple media items", field.Name.CamelFriendly()));
            }

            return Editor(part, field, shapeHelper);
        }

        //protected override void Importing(ContentPart part, Fields.MediaGalleryField field, ImportContentContext context) {
        //    var contentItemIds = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "ContentItems");
        //    if (contentItemIds != null) {
        //        field.Ids = contentItemIds.Split(',')
        //            .Select(context.GetItemFromSession)
        //            .Select(contentItem => contentItem.Id).ToArray();
        //    }
        //    else {
        //        field.Ids = new int[0];
        //    }
        //}

        //protected override void Exporting(ContentPart part, Fields.MediaGalleryField field, ExportContentContext context) {
        //    if (field.Ids.Any()) {
        //        var contentItemIds = field.Ids
        //            .Select(x => _contentManager.Get(x))
        //            .Select(x => _contentManager.GetItemMetadata(x).Identity.ToString())
        //            .ToArray();

        //        context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("ContentItems", string.Join(",", contentItemIds));
        //    }
        //}

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Items"), T("A Json serialized list of the media."));
        }
    }
}