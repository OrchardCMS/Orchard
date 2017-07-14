using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;
using System.Linq;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTermPartDriver : ContentPartDriver<TermPart> {
        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;

        public LocalizedTermPartDriver(
            IContentManager contentManager,
            ILocalizationService localizationService,
            INotifier notifier,
            ITaxonomyExtensionsService taxonomyExtensionsService) {
            _contentManager = contentManager;
            _localizationService = localizationService;
            _notifier = notifier;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        protected override string Prefix { get { return "LocalizedTerm"; } }

        protected override DriverResult Editor(TermPart termPart, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(termPart, Prefix, null, null)) {
                //If the term is localized and has a parent term I check if the term culture is the same of its parent culture
                if (termPart.Has<LocalizationPart>()) {
                    if (IsParentLocalized(termPart, updater)) {
                        //Retrieving the parent taxonomy and the parent term
                        ContentItem parentTerm = _taxonomyExtensionsService.GetParentTerm(termPart);
                        ContentItem parentTaxonomy = _taxonomyExtensionsService.GetParentTaxonomy(termPart);

                        if (termPart.As<LocalizationPart>().Culture != null) {
                            CultureRecord termCulture = termPart.As<LocalizationPart>().Culture;
                            CultureRecord taxonomyCulture = null;

                            //If a parent term exists I retrieve its localized version.
                            ContentItem localizedParentTerm = parentTerm;
                            if (parentTerm != null) {
                                if (parentTerm.Has<LocalizationPart>()) {
                                    CultureRecord parentTermCulture = parentTerm.As<LocalizationPart>().Culture;

                                    if (parentTermCulture != null && termCulture.Id != parentTermCulture.Id) {
                                        IContent masterParentTerm = _taxonomyExtensionsService.GetMasterItem(parentTerm);
                                        var localizedParent = _localizationService.GetLocalizedContentItem(masterParentTerm, termCulture.Culture);
                                        if (localizedParent != null)
                                            localizedParentTerm = localizedParent.As<ContentItem>();
                                    }
                                }
                            }

                            //Retrieving the localized version of the Taxonomy
                            ContentItem localizedParentTaxonomy = parentTaxonomy;
                            if (parentTaxonomy.Has<LocalizationPart>()) {
                                if (parentTaxonomy.As<LocalizationPart>().Culture != null) {
                                    taxonomyCulture = parentTaxonomy.As<LocalizationPart>().Culture;
                                    if (termCulture.Id != taxonomyCulture.Id) {
                                        IContent masterTaxonomy = _taxonomyExtensionsService.GetMasterItem(parentTaxonomy);
                                        var localizedTaxonomy = _localizationService.GetLocalizedContentItem(masterTaxonomy, termCulture.Culture);
                                        if (localizedTaxonomy != null)
                                            localizedParentTaxonomy = localizedTaxonomy.As<ContentItem>();
                                    }
                                }
                            }

                            //Assigning to the term the corresponding localized container
                            if ((localizedParentTerm == null && localizedParentTaxonomy != parentTaxonomy) || (localizedParentTerm != null && localizedParentTerm != parentTerm && localizedParentTerm.Id != termPart.Id)) {
                                termPart.Container = localizedParentTerm == null ? localizedParentTaxonomy.As<TaxonomyPart>().ContentItem : localizedParentTerm.As<TermPart>().ContentItem;
                                termPart.Path = localizedParentTerm != null ? localizedParentTerm.As<TermPart>().FullPath + "/" : "/";
                                if (localizedParentTerm == null)
                                    termPart.TaxonomyId = localizedParentTaxonomy.Id;
                                else
                                    termPart.TaxonomyId = localizedParentTerm.As<TermPart>().TaxonomyId;

                                if (localizedParentTaxonomy != parentTaxonomy)
                                    _notifier.Add(NotifyType.Information, T("The parent taxonomy has been changed to its localized version associated to the culture {0}.", localizedParentTaxonomy.As<LocalizationPart>().Culture.Culture));

                                if (localizedParentTerm != null && localizedParentTerm != parentTerm)
                                    _notifier.Add(NotifyType.Information, T("The parent term has been changed to its localized version associated to the culture {0}.", localizedParentTerm.As<LocalizationPart>().Culture.Culture));
                            }
                            else if (termCulture != taxonomyCulture && taxonomyCulture != null && _localizationService.GetLocalizations(parentTaxonomy).Count() > 0) {
                                //I can associate to a taxonomy a term of a different culture only if the taxonomy is unlocalized or has no translations
                                updater.AddModelError("WrongTaxonomyLocalization", T("A localization of the taxonomy in the specified language does not exist. Please create it first."));
                            }
                        }
                    }
                }
            }

            return null;
        }

        private bool IsParentLocalized(TermPart termPart, IUpdateModel updater) {
            bool isLocalized = true;

            //Checking if the term has a parent term
            ContentItem parentTerm = null;
            var container = _contentManager.Get(termPart.Container.Id);
            if (container.ContentType != "Taxonomy")
                parentTerm = container;

            if (parentTerm != null) {
                if (parentTerm.Has<LocalizationPart>()) {
                    var termCulture = termPart.As<LocalizationPart>().Culture;
                    var parentTermCulture = parentTerm.As<LocalizationPart>().Culture;

                    if (termCulture != null) {
                        //If the parent is not localized it must be localized first
                        if (parentTermCulture == null) {
                            isLocalized = false;
                            updater.AddModelError("MissingParentTermLocalization", T("The parent term is not localized. Please localize it first."));
                        }
                        else {
                            //If the two cultures are different, I check if the parent has a translation with the same culture of the new term
                            if (termCulture != parentTermCulture) {
                                //If it doesn't exists the term cannot be saved
                                if (_localizationService.GetLocalizedContentItem(parentTerm, termCulture.Culture) == null) {
                                    isLocalized = false;
                                    updater.AddModelError("WrongParentTermLocalization", T("A localization of the parent term in the specified language does not exist. Please create it first."));
                                }
                            }
                        }
                    }
                }
            }

            return isLocalized;
        }
    }
}