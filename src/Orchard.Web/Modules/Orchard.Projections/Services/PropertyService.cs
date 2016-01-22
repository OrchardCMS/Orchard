using System.Linq;
using Orchard.Data;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public class PropertyService : IPropertyService {
        private readonly IRepository<PropertyRecord> _repository;

        public PropertyService(IRepository<PropertyRecord> repository) {
            _repository = repository;
        }

        public void MoveUp(int propertyId) {
            var property = _repository.Get(propertyId);

            // look for the previous action in order in same rule
            var previous = _repository.Table
                .Where(x => x.Position < property.Position && x.LayoutRecord.Id == property.LayoutRecord.Id)
                .OrderByDescending(x => x.Position)
                .FirstOrDefault();

            // nothing to do if already at the top
            if (previous == null) {
                return;
            }

            // switch positions
            var temp = previous.Position;
            previous.Position = property.Position;
            property.Position = temp;
        }

        public void MoveDown(int propertyId) {
            var property = _repository.Get(propertyId);

            // look for the next action in order in same rule
            var next = _repository.Table
                .Where(x => x.Position > property.Position && x.LayoutRecord.Id == property.LayoutRecord.Id)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            // nothing to do if already at the end
            if (next == null) {
                return;
            }

            // switch positions
            var temp = next.Position;
            next.Position = property.Position;
            property.Position = temp;
        }
    }
}