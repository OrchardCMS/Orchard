using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyPartDriver : ContentPartDriver<TaxonomyPart> {
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;

        public LocalizedTaxonomyPartDriver(ITaxonomyExtensionsService taxonomyExtensionsService) {
            _taxonomyExtensionsService = taxonomyExtensionsService;
        }

        protected override string Prefix { get { return "LocalizedTaxonomy"; } }

        protected override DriverResult Editor(TaxonomyPart part, dynamic shapeHelper) {
            AssociateTermTypeViewModel model = new AssociateTermTypeViewModel();
            model.TermTypes = _taxonomyExtensionsService.GetAllTermTypes();
            model.TermCreationAction = TermCreationOptions.CreateLocalized;
            model.SelectedTermTypeId = part.TermTypeName;
            model.ContentItem = part;

            return ContentShape("Parts_TaxonomyTermSelector",
                                () => shapeHelper.EditorTemplate(
                                          TemplateName: "Parts/TaxonomyTermSelector",
                                          Model: model,
                                          Prefix: Prefix));
        }

        protected override DriverResult Editor(TaxonomyPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (string.IsNullOrWhiteSpace(part.TermTypeName)) {
                AssociateTermTypeViewModel model = new AssociateTermTypeViewModel();
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                    switch (model.TermCreationAction) {
                        case TermCreationOptions.CreateLocalized:
                            _taxonomyExtensionsService.CreateLocalizedTermContentType(part);
                            break;
                        case TermCreationOptions.UseExisting:
                            part.TermTypeName = model.SelectedTermTypeId;
                            break;
                        default:
                            part.TermTypeName = null;
                            break;
                    }
                }
            }

            return Editor(part, shapeHelper);
        }
    }
}