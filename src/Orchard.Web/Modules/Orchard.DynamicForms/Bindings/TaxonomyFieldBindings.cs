using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;

namespace Orchard.DynamicForms.Bindings {
    [OrchardFeature("Orchard.DynamicForms.Taxonomies")]
    public class TaxonomyFieldBindings : Component, IBindingProvider {
        private readonly ITaxonomyService _taxonomyService;
        public TaxonomyFieldBindings(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        public void Describe(BindingDescribeContext context) {
            context.For<TaxonomyField>()
                .Binding("Terms", (contentItem, field, s) => {
                    var selectedTerms = 
                        s.Split(new []{',', ';'}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(XmlHelper.Parse<int?>)
                        .Select(t => GetTerm(t.GetValueOrDefault()))
                        .Where(t => t != null).ToList();

                    _taxonomyService.UpdateTerms(contentItem, selectedTerms, field.Name);
                });
        }

        private TermPart GetTerm(int termId) {
            return _taxonomyService.GetTerm(termId);
        }
    }
}