using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    public class ElementBlueprintService : IElementBlueprintService {
        private readonly IRepository<ElementBlueprint> _blueprintRepository;
        private readonly ISignals _signals;

        public ElementBlueprintService(IRepository<ElementBlueprint> blueprintRepository, ISignals signals) {
            _blueprintRepository = blueprintRepository;
            _signals = signals;
        }

        public ElementBlueprint GetBlueprint(int id) {
            return _blueprintRepository.Get(id);
        }

        public IEnumerable<ElementBlueprint> GetBlueprints() {
            return _blueprintRepository.Table;
        }

        public void DeleteBlueprint(ElementBlueprint blueprint) {
            _blueprintRepository.Delete(blueprint);
            _signals.Trigger(Signals.ElementDescriptors);
        }

        public int DeleteBlueprints(IEnumerable<int> ids) {
            var blueprints = _blueprintRepository.Table.Where(x => ids.Contains(x.Id)).ToArray();

            foreach (var blueprint in blueprints) {
                DeleteBlueprint(blueprint);
            }

            return blueprints.Length;
        }

        public ElementBlueprint CreateBlueprint(Element baseElement, string elementTypeName, string elementDisplayName, string elementDescription, string elementCategory) {
            var blueprint = new ElementBlueprint {
                BaseElementTypeName = baseElement.Descriptor.TypeName,
                ElementTypeName = elementTypeName,
                ElementDisplayName = elementDisplayName,
                ElementDescription = elementDescription,
                ElementCategory = elementCategory
            };

            _blueprintRepository.Create(blueprint);
            return blueprint;
        }
    }
}