using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public class BindingManager : IBindingManager {
        private readonly IEnumerable<IBindingProvider> _providers;
        public BindingManager(IEnumerable<IBindingProvider> providers) {
            _providers = providers;
        }

        public IEnumerable<BindingContext> DescribeBindingContexts() {
            foreach (var provider in _providers) {
                var context = new BindingDescribeContext();
                provider.Describe(context);

                foreach (var description in context.Describe()) {
                    yield return description;
                }
            }
        }

        public IEnumerable<ContentPartBindingDescriptor> DescribeBindingsFor(ContentTypeDefinition contentTypeDefinition) {
            var contexts = DescribeBindingContexts().ToLookup(x => x.ContextName);

            foreach (var part in contentTypeDefinition.Parts) {
                var partName = part.PartDefinition.Name;
                var partBinding = new ContentPartBindingDescriptor() {
                    Part = part,
                    BindingContexts = contexts[partName].ToList()
                };

                foreach (var field in part.PartDefinition.Fields) {
                    var fieldName = field.FieldDefinition.Name;
                    var fieldBinding = new ContentFieldBindingDescriptor {
                        Field = field,
                        BindingContexts = contexts[fieldName].ToList()
                    };

                    partBinding.FieldBindings.Add(fieldBinding);
                }

                yield return partBinding;
            }
        }
    }
}