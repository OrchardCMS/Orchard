using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Settings;
using Orchard.UI.Notify;

namespace Orchard.MediaLibrary.Handlers {
    [OrchardFeature("Orchard.MediaLibrary.LocalizationExtensions")]
    public class MediaLibraryPickerFieldLocalizationExtensionHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationServices;
        private readonly IOrchardServices _orchardServices;

        public MediaLibraryPickerFieldLocalizationExtensionHandler(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            ILocalizationService localizationServices) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _localizationServices = localizationServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            //Here we implement the logic based on the settings introduced in MediaLibraryPickerFieldLocalizationSettings
            //These settings should only be active if the ContentItem that is being updated has a LocalizationPart
            if (context.ContentItem.Parts.Any(part => part is LocalizationPart)) {
                var lPart = (LocalizationPart)context.ContentItem.Parts.Single(part => part is LocalizationPart);
                var fields = context.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(MediaLibraryPickerField).Name)).Cast<MediaLibraryPickerField>();
                var contentCulture = context.ContentItem.As<LocalizationPart>().Culture != null ? context.ContentItem.As<LocalizationPart>().Culture.Culture : null;
                foreach (var field in fields) {
                    var fieldSettings = field.PartFieldDefinition.Settings.GetModel<MediaLibraryPickerFieldSettings>();
                    var settings = field.PartFieldDefinition.Settings.GetModel<MediaLibraryPickerFieldLocalizationSettings>();

                    if (settings.TryToLocalizeMedia) {
                        //try to replace items in the field with their translation
                        var itemsInField = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Latest, QueryHints.Empty);
                        var mediaIds = new List<int>();
                        foreach (var item in itemsInField) {
                            // negatives id whoud be localized
                            var mediaItem = _contentManager.Get(item.Id, VersionOptions.Latest);
                            var mediaIsLocalizable = mediaItem.As<LocalizationPart>() != null;
                            var mediaCulture = mediaIsLocalizable && mediaItem.As<LocalizationPart>().Culture != null ? mediaItem.As<LocalizationPart>().Culture.Culture : null;
                            if (mediaItem != null && mediaIsLocalizable) {
                                // The media is localizable
                                if (contentCulture == mediaCulture) {
                                    // The content culture and the media culture match
                                    mediaIds.Add(mediaItem.Id);
                                }
                                else {
                                    if (mediaCulture == null) {
                                        // The media has not a culture, so it takes the content culture
                                        _localizationServices.SetContentCulture(mediaItem, contentCulture);
                                        mediaIds.Add(mediaItem.Id);
                                        _orchardServices.Notifier.Warning(T(
                                            "{0}: the media item {1} was culture neutral and it has been localized",
                                            field.DisplayName,
                                            mediaItem.As<MediaPart>().FileName));
                                    }
                                    else {
                                        // The media has a culture
                                        var localizedMedia = _localizationServices.GetLocalizedContentItem(mediaItem, contentCulture);
                                        if (localizedMedia != null) {
                                            // The media has a translation, so the field will replace current media with the right localized one.
                                            mediaIds.Add(localizedMedia.Id);
                                            _orchardServices.Notifier.Warning(T(
                                                "{0}: the media item {1} has been replaced by its localized version",
                                                field.DisplayName,
                                                mediaItem.As<MediaPart>().FileName));
                                        }
                                        else {
                                            if (!settings.RemoveItemsWithoutLocalization) {
                                                // The media supports translations but have not a localized version, so it will be cloned in the right language
                                                var clonedMedia = _contentManager.Clone(mediaItem);
                                                var mediaLocalizationPart = mediaItem.As<LocalizationPart>();
                                                if (mediaLocalizationPart != null) {
                                                    _localizationServices.SetContentCulture(clonedMedia, contentCulture);
                                                    clonedMedia.As<LocalizationPart>().MasterContentItem = mediaLocalizationPart.MasterContentItem == null ? mediaItem : mediaLocalizationPart.MasterContentItem;
                                                }
                                                _contentManager.Publish(clonedMedia);
                                                mediaIds.Add(clonedMedia.Id);
                                                _orchardServices.Notifier.Warning(T(
                                                    "{0}: a localized version of media item {1} has been created",
                                                    field.DisplayName,
                                                    mediaItem.As<MediaPart>().FileName));
                                            }
                                            else {
                                                _orchardServices.Notifier.Warning(T(
                                                    "{0}: the media item {1} has been removed from the field because its culture differs from content's culture",
                                                    field.DisplayName,
                                                    mediaItem.As<MediaPart>().FileName));
                                            }
                                        }
                                    }
                                }
                            }
                            else if (mediaItem != null && !mediaIsLocalizable) {
                                if (!settings.RemoveItemsWithNoLocalizationPart) {
                                    mediaIds.Add(mediaItem.Id);
                                }
                                else {
                                    _orchardServices.Notifier.Warning(T(
                                        "{0}: the media item {1} has been removed from the field because culture neutral",
                                        field.DisplayName,
                                        mediaItem.As<MediaPart>().FileName));
                                }
                            }
                        }

                        field.Ids = mediaIds.Distinct().ToArray();

                        if (field.Ids.Length == 0 && fieldSettings.Required) {
                            context.Updater.AddModelError("Id", T("The {0} field is required.", field.DisplayName));
                        }
                    }
                }
            }
        }
    }
}