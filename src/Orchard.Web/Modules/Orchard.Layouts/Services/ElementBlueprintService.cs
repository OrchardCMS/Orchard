using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    public class ElementBlueprintService : IElementBlueprintService {
        private readonly IRepository<ElementBlueprint> _blueprintRepository;
        public ElementBlueprintService(IRepository<ElementBlueprint> blueprintRepository) {
            _blueprintRepository = blueprintRepository;
        }

        public ElementBlueprint GetBlueprint(int id) {
            return _blueprintRepository.Get(id);
        }

        public IEnumerable<ElementBlueprint> GetBlueprints() {
            return _blueprintRepository.Table;
        }

        public void DeleteBlueprint(ElementBlueprint blueprint) {
            _blueprintRepository.Delete(blueprint);
        }

        public int DeleteBlueprints(IEnumerable<int> ids) {
            var blueprints = _blueprintRepository.Table.Where(x => ids.Contains(x.Id)).ToArray();

            foreach (var blueprint in blueprints) {
                DeleteBlueprint(blueprint);
            }

            return blueprints.Length;
        }

        public ElementBlueprint CreateBlueprint(IElement baseElement, string elementTypeName, string elementDisplayName, string elementCategory) {
            var blueprint = new ElementBlueprint {
                BaseElementTypeName = baseElement.Descriptor.TypeName,
                ElementTypeName = elementTypeName,
                ElementDisplayName = elementDisplayName,
                ElementCategory = elementCategory
            };

            _blueprintRepository.Create(blueprint);
            return blueprint;
        }
    }
}