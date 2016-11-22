using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly IEnumerable<IContentFieldDriver> _fieldDrivers;
        private readonly IEnumerable<IContentPartDriver> _partDrivers;
        public LocalizationCultureNeutralityHandler(ILocalizationSetService localizationSetService,
            IEnumerable<IContentFieldDriver> fieldDrivers,
            IEnumerable<IContentPartDriver> partDrivers) {
            _localizationSetService = localizationSetService;
            _fieldDrivers = fieldDrivers;
            _partDrivers = partDrivers;

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
                    if (pa.PartDefinition.Settings.GetModel<LocalizationCultureNeutralitySettings>().AllowSynchronization
                        && pa.Settings.GetModel<LocalizationCultureNeutralitySettings>().CultureNeutral) {
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
            if (lSet.Count > 0) {
                var partDrivers = _partDrivers.Where(cpd => cpd.GetPartInfo().FirstOrDefault().PartName == part.PartDefinition.Name);
                //implement the cloning fallback here like in the driver coordinators
                bool noCloningImplementation = true;
                foreach (var contentPartDriver in partDrivers.Where(cpd => cpd is IContentPartCloningDriver)) {
                    if (contentPartDriver.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(mi => mi.Name == "Cloning").FirstOrDefault() != null) {
                        noCloningImplementation = false;
                        break;
                    }
                }
                if (noCloningImplementation) {
                    var ecc = new ExportContentContext(localizationPart.ContentItem, new System.Xml.Linq.XElement(System.Xml.XmlConvert.EncodeLocalName(localizationPart.ContentItem.ContentType)));
                    partDrivers.Invoke(driver => driver.Exporting(ecc), ecc.Logger);
                    partDrivers.Invoke(driver => driver.Exported(ecc), ecc.Logger);
                    var importContentSession = new ImportContentSession(ecc.ContentManager);
                    foreach (var target in lSet.Select(lp => lp.ContentItem)) {
                        var copyId = target.Id.ToString();
                        importContentSession.Set(copyId, ecc.Data.Name.LocalName);
                        var icc = new ImportContentContext(target, ecc.Data, importContentSession);
                        partDrivers.Invoke(driver => driver.Importing(icc), icc.Logger);
                        partDrivers.Invoke(driver => driver.Imported(icc), icc.Logger);
                        partDrivers.Invoke(driver => driver.ImportCompleted(icc), icc.Logger);
                    }
                }
                else {
                    var cloningDrivers = partDrivers.Select(cpd => cpd as IContentPartCloningDriver).Where(cpd => cpd != null);
                    foreach (var target in lSet.Select(lp => lp.ContentItem)) {
                        var context = new CloneContentContext(localizationPart.ContentItem, target);
                        cloningDrivers.Invoke(driver => driver.Cloning(context), context.Logger);
                        cloningDrivers.Invoke(driver => driver.Cloned(context), context.Logger);
                    }
                }
            }
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
                var fieldDrivers = _fieldDrivers.Where(cfd => cfd.GetFieldInfo().FirstOrDefault().FieldTypeName == field.FieldDefinition.Name);
                //Implement the cloning fallback here like in the driver coordinators
                bool noCloningImplementation = true;
                foreach (var contentFieldDriver in fieldDrivers.Where(cfd => cfd is IContentFieldCloningDriver)) {
                    //if we find an implementation of cloning, break
                    if (contentFieldDriver.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(mi => mi.Name == "Cloning").FirstOrDefault() != null) {
                        noCloningImplementation = false;
                        break;
                    }
                }
                if (noCloningImplementation) {
                    //fallback
                    var ecc = new ExportContentContext(localizationPart.ContentItem, new System.Xml.Linq.XElement(System.Xml.XmlConvert.EncodeLocalName(localizationPart.ContentItem.ContentType)));
                    ecc.FieldName = field.Name;
                    fieldDrivers.Invoke(driver => driver.Exporting(ecc), ecc.Logger);
                    fieldDrivers.Invoke(driver => driver.Exported(ecc), ecc.Logger);
                    var importContentSession = new ImportContentSession(ecc.ContentManager);
                    foreach (var target in lSet.Select(lp => lp.ContentItem)) {
                        var copyId = target.Id.ToString();
                        importContentSession.Set(copyId, ecc.Data.Name.LocalName);
                        var icc = new ImportContentContext(target, ecc.Data, importContentSession);
                        icc.FieldName = field.Name;
                        fieldDrivers.Invoke(driver => driver.Importing(icc), icc.Logger);
                        fieldDrivers.Invoke(driver => driver.Imported(icc), icc.Logger);
                        fieldDrivers.Invoke(driver => driver.ImportCompleted(icc), icc.Logger);
                    }
                }
                else {
                    var cloningDrivers = fieldDrivers.Select(cfd => cfd as IContentFieldCloningDriver).Where(cfd => cfd != null);
                    foreach (var target in lSet.Select(lp => lp.ContentItem)) {
                        var context = new CloneContentContext(localizationPart.ContentItem, target);
                        context.FieldName = field.Name;
                        cloningDrivers.Invoke(driver => driver.Cloning(context), context.Logger);
                        cloningDrivers.Invoke(driver => driver.Cloned(context), context.Logger);
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