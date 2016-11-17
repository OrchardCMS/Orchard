using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.ViewModels;
using System.Linq;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyPartDriver : ContentPartDriver<TaxonomyPart> {
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;

        public LocalizedTaxonomyPartDriver(ITaxonomyExtensionsService taxonomyExtensionsService) {
            _taxonomyExtensionsService = taxonomyExtensionsService;
        }

        protected override string Prefix { get { return "LocalizedTaxonomy"; } }

        protected override DriverResult Editor(TaxonomyPart part, dynamic shapeHelper) {
            var model = new AssociateTermTypeViewModel();
            model.TermTypes = _taxonomyExtensionsService.GetAllTermTypes();

            model.SelectedTermTypeId = part.TermTypeName != null ? part.TermTypeName : TermCreationAction.CreateLocalized.ToString();

            return ContentShape("Parts_TaxonomyTermSelector",
                                () => shapeHelper.EditorTemplate(
                                          TemplateName: "Parts/TaxonomyTermSelector",
                                          Model: model,
                                          Prefix: Prefix));
        }

        protected override DriverResult Editor(TaxonomyPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
    }
}