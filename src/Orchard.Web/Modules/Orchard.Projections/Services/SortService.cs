using System.Linq;
using Orchard.Data;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public class SortService : ISortService {
        private readonly IRepository<SortCriterionRecord> _repository;

        public SortService(IRepository<SortCriterionRecord> repository) {
            _repository = repository;
        }

        public void MoveUp(int propertyId) {
            var property = _repository.Get(propertyId);

            // look for the previous action in order in same query
            var previous = _repository.Table
                .Where(x => x.Position < property.Position && x.QueryPartRecord.Id == property.QueryPartRecord.Id)
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

            // look for the next action in order in same query
            var next = _repository.Table
                .Where(x => x.Position > property.Position && x.QueryPartRecord.Id == property.QueryPartRecord.Id)
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