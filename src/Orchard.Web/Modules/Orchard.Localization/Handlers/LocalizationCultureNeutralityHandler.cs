using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Localization.Settings;

namespace Orchard.Localization.Handlers {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public class LocalizationCultureNeutralityHandler : ContentHandler {
        private readonly ILocalizationSetService _localizationSetService;
        private readonly IEnumerable<IContentFieldCloningDriver> _cloningFieldDrivers;
        private readonly IEnumerable<IContentPartCloningDriver> _cloningPartDrivers;
        public LocalizationCultureNeutralityHandler(ILocalizationSetService localizationSetService,
            IEnumerable<IContentFieldCloningDriver> cloningFieldDrivers,
            IEnumerable<IContentPartCloningDriver> cloningPartDrivers) {
            _localizationSetService = localizationSetService;
            _cloningFieldDrivers = cloningFieldDrivers;
            _cloningPartDrivers = cloningPartDrivers;

            OnPublished<IContent>(SynchronizeOnPublish);
        }

        protected void SynchronizeOnPublish(PublishContentContext context, IContent part) {
            //Conditions to try and start a synchronization:
            // && The content item is localizable
            //    - The content item has a LocalizationPart
            //    - We can check this on either the type or the item itself
            // && The part has the CultureNeutral setting set to true
            //After eventually synchronizing the part, we check whether we should be synchronizing any of its fields
            // - Go through all the fields and check the CultureNeutral setting.
            var locPart = part.ContentItem.As<LocalizationPart>();
            if (locPart != null) {
                //cycle through all parts
                foreach (var pa in part.ContentItem.Parts) {
                    if (pa.Settings.GetModel<LocalizationCultureNeutralitySettings>().CultureNeutral) {
                        Synchronize(pa, locPart);
                    }
                    foreach (var field in pa.Fields.Where(fi => fi.PartFieldDefinition.Settings.GetModel<LocalizationCultureNeutralitySettings>().CultureNeutral)) {
                        Synchronize(field, locPart);
                    }
                }
            }
        }

        /// <summary>
        /// This method attempts to synchronize a part across the localization set
        /// </summary>
        /// <param name="part">The part that has just been published and that we wish to use to update all corresponding parts from
        /// the other elements of the localization set.</param>
        /// <param name="localizationPart">The localization part of the ContentItem that was just published.</param>
        private void Synchronize(ContentPart part, LocalizationPart localizationPart) {
            //given the LocalizationPart, get the localization set
            var lSet = GetSynchronizationSet(localizationPart);
        }
        /// <summary>
        /// This method attempts to synchronize a field across the localization set
        /// </summary>
        /// <param name="field">The field that has just been published and that we wish to use to update all corresponding parts from
        /// the other elements of the localization set.</param>
        /// <param name="localizationPart">The localization part of the ContentItem that was just published.</param>
        private void Synchronize(ContentField field, LocalizationPart localizationPart) {
            var lSet = GetSynchronizationSet(localizationPart);
            if (lSet.Count > 0) {
                var fieldDrivers = _cloningFieldDrivers.Where(cfd => cfd.GetFieldInfo().FirstOrDefault().FieldTypeName == field.FieldDefinition.Name);
                foreach (var target in lSet.Select(lp => lp.ContentItem)) {
                    var context = new CloneContentContext(localizationPart.ContentItem, target);
                    foreach (var driver in fieldDrivers) {
                        driver.Cloning(context);
                    }
                    foreach (var driver in fieldDrivers) {
                        driver.Cloned(context);
                    }
                }
            }
        }

        private List<LocalizationPart> GetSynchronizationSet(LocalizationPart lPart) {
            var lSet = _localizationSetService.GetLocalizationSet(
                localizationSetId: lPart.LocalizationSetId,
                versionOptions: VersionOptions.Published).ToList();
            lSet.AddRange(_localizationSetService.GetLocalizationSet(
                localizationSetId: lPart.LocalizationSetId,
                versionOptions: VersionOptions.Latest));
            return lSet.Distinct().Where(lp => lp.Id != lPart.Id).ToList();
        }
    }
}