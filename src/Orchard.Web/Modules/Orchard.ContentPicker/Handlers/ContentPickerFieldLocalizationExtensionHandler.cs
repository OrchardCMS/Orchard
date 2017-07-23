using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentPicker.Fields;
using Orchard.ContentPicker.Settings;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;

namespace Orchard.ContentPicker.Handlers {
    [OrchardFeature("Orchard.ContentPicker.LocalizationExtensions")]
    public class ContentPickerFieldLocalizationExtensionHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        public Localizer T { get; set; }

        public ContentPickerFieldLocalizationExtensionHandler(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            ILocalizationService localizationService) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _localizationService = localizationService;

            T = NullLocalizer.Instance;
        }

        protected override void UpdateEditorShape(UpdateEditorContext context) {
            base.UpdateEditorShape(context);
            //Here we implement the logic based on the settings introduced in ContentPickerFieldLocalizationSettings
            //These settings should only be active if the ContentItem that is being updated has a LocalizationPart
            if (context.ContentItem.Parts.Any(part => part is LocalizationPart)) {
                var lPart = (LocalizationPart)context.ContentItem.Parts.Single(part => part is LocalizationPart);
                var fields = context.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(ContentPickerField).Name)).Cast<ContentPickerField>();

                foreach (var field in fields) {
                    var settings = field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldLocalizationSettings>();
                    if (settings.TryToLocalizeItems) {
                        //try to replace items in the field with their translation
                        var itemsInField = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Latest, QueryHints.Empty);
                        if (settings.RemoveItemsWithNoLocalizationPart && itemsInField.Where(ci => !ci.Parts.Any(part => part is LocalizationPart)).Any()) {
                            //keep only items that have a LocalizationPart
                            _orchardServices.Notifier.Warning(T(
                                "{0}: The following items could have no localization, so they were removed: {1}",
                                field.DisplayName,
                                string.Join(", ", itemsInField.Where(ci => !ci.Parts.Any(part => part is LocalizationPart)).Select(ci => _contentManager.GetItemMetadata(ci).DisplayText))
                                ));
                            itemsInField = itemsInField.Where(ci => ci.Parts.Any(part => part is LocalizationPart));
                        }
                        //use an (int, int) tuple to track translations
                        var newIds = itemsInField.Select(ci => {
                            if (ci.Parts.Any(part => part is LocalizationPart)) {
                                if (_localizationService.GetContentCulture(ci) == lPart.Culture.Culture)
                                    return new Tuple<int, int>(ci.Id, ci.Id); //this item is fine
                                var localized = _localizationService.GetLocalizations(ci).FirstOrDefault(lp => lp.Culture == lPart.Culture);
                                return localized == null ? new Tuple<int, int>(ci.Id, -ci.Id) : new Tuple<int, int>(ci.Id, localized.Id); //return negative id where we found no translation
                            }
                            else {
                                //we only go here if RemoveItemsWithNoLocalizationPart == false
                                return new Tuple<int, int>(ci.Id, ci.Id);
                            }
                        });
                        if (newIds.Any(tup => tup.Item2 < 0)) {
                            if (settings.RemoveItemsWithoutLocalization) {
                                //remove the items for which we could not find a localization
                                _orchardServices.Notifier.Warning(T(
                                    "{0}: We could not find a localization for the following items, so they were removed: {1}",
                                    field.DisplayName,
                                    string.Join(", ", newIds.Where(tup => tup.Item2 < 0).Select(tup => _contentManager.GetItemMetadata(_contentManager.GetLatest(tup.Item1)).DisplayText))
                                    ));
                                newIds = newIds.Where(tup => tup.Item2 > 0);
                            }
                            else {
                                //negative Ids are made positive again
                                newIds = newIds.Select(tup => tup = new Tuple<int, int>(tup.Item1, Math.Abs(tup.Item2)));
                            }
                        }
                        if (newIds.Where(tup => tup.Item1 != tup.Item2).Any()) {
                            _orchardServices.Notifier.Warning(T(
                                "{0}: The following items were replaced by their correct localization: {1}",
                                field.DisplayName,
                                string.Join(", ", newIds.Where(tup => tup.Item1 != tup.Item2).Select(tup => _contentManager.GetItemMetadata(_contentManager.GetLatest(tup.Item1)).DisplayText))
                                ));
                        }

                        field.Ids = newIds.Select(tup => tup.Item2).Distinct().ToArray();
                    }
                    if (settings.AssertItemsHaveSameCulture) {
                        //verify that the items in the ContentPickerField are all in the culture of the ContentItem whose editor we are updating
                        var itemsInField = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Latest, QueryHints.Empty);
                        var itemsWithoutLocalizationPart = itemsInField.Where(ci => !ci.Parts.Any(part => part is LocalizationPart));
                        List<int> badItemIds = itemsInField.Where(ci => ci.Parts.Any(part => part is LocalizationPart && ((LocalizationPart)part).Culture != lPart.Culture)).Select(ci => ci.Id).ToList();
                        if (itemsWithoutLocalizationPart.Count() > 0) {
                            //Verify items from the ContentPickerField that cannot be localized
                            _orchardServices.Notifier.Warning(T("{0}: Some of the selected items cannot be localized: {1}",
                                field.DisplayName,
                                string.Join(", ", itemsWithoutLocalizationPart.Select(ci => _contentManager.GetItemMetadata(ci).DisplayText))
                                ));
                            if (settings.BlockForItemsWithNoLocalizationPart) {
                                badItemIds.AddRange(itemsWithoutLocalizationPart.Select(ci => ci.Id));
                            }
                        }
                        if (badItemIds.Count > 0) {
                            context.Updater.AddModelError(field.DisplayName, T("Some of the items selected have the wrong localization: {0}",
                                string.Join(", ", badItemIds.Select(id => _contentManager.GetItemMetadata(_contentManager.GetLatest(id)).DisplayText))
                                ));
                        }
                    }
                }
            }
        }

    }
}
