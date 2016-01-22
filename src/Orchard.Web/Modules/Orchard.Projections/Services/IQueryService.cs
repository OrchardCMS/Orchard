using System.Linq;
using Orchard.ContentManagement;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public interface IQueryService : IDependency {
        QueryPart GetQuery(int id);

        QueryPart CreateQuery(string name);
        void DeleteQuery(int id);
    }
}