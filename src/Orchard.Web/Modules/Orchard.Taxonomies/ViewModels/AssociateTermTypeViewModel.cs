using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;

namespace Orchard.Taxonomies.ViewModels {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class AssociateTermTypeViewModel {
        public IEnumerable<ContentTypeDefinition> TermTypes { get; set; }
        public string SelectedTermTypeId { get; set; }
    }

    public enum TermCreationAction {
        CreateCultureNeutral = 1,
        CreateLocalized = 2
    }
}