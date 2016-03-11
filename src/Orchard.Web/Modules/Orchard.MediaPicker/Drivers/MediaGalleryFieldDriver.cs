using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
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

        protected override void Importing(ContentPart part, MediaGalleryField field, ImportContentContext context) {
            var mediaItems = new List<MediaGalleryItem>();
            var root = context.Data.Element(field.FieldDefinition.Name + "." + field.Name);

            if (root == null) {
                return;
            }

            foreach (var element in root.Elements("MediaItem")) {
                mediaItems.Add(new MediaGalleryItem {
                    Url = element.Attribute("Url").Value,
                    AlternateText = element.Attribute("AlternateText").Value,
                    Class = element.Attribute("Class").Value,
                    Style = element.Attribute("Style").Value,
                    Alignment = element.Attribute("Alignment").Value,
                    Width = int.Parse(element.Attribute("Width").Value, CultureInfo.InvariantCulture),
                    Height = int.Parse(element.Attribute("Height").Value, CultureInfo.InvariantCulture),
                });
            }

            field.SelectedItems = mediaItems.Any() ? _jsonConverter.Serialize(mediaItems.ToArray()) : "[]";
        }

        protected override void Exporting(ContentPart part, MediaGalleryField field, ExportContentContext context) {
            var element = context.Element(field.FieldDefinition.Name + "." + field.Name);

            foreach (var mediaItem in field.Items) {
                element.Add(new XElement("MediaItem",
                    new XAttribute("Url", mediaItem.Url),
                    new XAttribute("AlternateText", mediaItem.AlternateText),
                    new XAttribute("Class", mediaItem.Class),
                    new XAttribute("Style", mediaItem.Style),
                    new XAttribute("Alignment", mediaItem.Alignment),
                    new XAttribute("Width", mediaItem.Width),
                    new XAttribute("Height", mediaItem.Height)
                    )
                );
            }
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Items"), T("A Json serialized list of the media."));
        }
    }
}