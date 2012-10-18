using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public class QueryService : IQueryService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<QueryPartRecord> _repository;

        public QueryService(IContentManager contentManager, IRepository<QueryPartRecord> repository) {
            _contentManager = contentManager;
            _repository = repository;
        }

        public Localizer T { get; set; }

        public QueryPart CreateQuery(string name) {
            var contentItem = _contentManager.New("Query");
            var query = contentItem.As<QueryPart>();
            query.Name = name;

            _contentManager.Create(contentItem);

            return query;
        }

        public QueryPart GetQuery(int id) {
            return _contentManager.Get<QueryPart>(id);
        }

        public void DeleteQuery(int id) {
            var query = _contentManager.Get(id);
            
            if (query != null) {
                _contentManager.Remove(query);
            }
        }
    }
}