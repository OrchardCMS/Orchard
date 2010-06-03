using System.Collections.Generic;
using System.Linq;

namespace Orchard.Indexing {
    public class DefaultIndexManager : IIndexManager {

        private readonly IEnumerable<IIndexProvider> _indexProviders;

        public DefaultIndexManager(IEnumerable<IIndexProvider> indexProviders) {
            _indexProviders = indexProviders;
        }

        #region IIndexManager Members

        public bool HasIndexProvider() {
            return _indexProviders.AsQueryable().Count() > 0;
        }

        public IIndexProvider GetSearchIndexProvider() {
            return _indexProviders.AsQueryable().FirstOrDefault();
        }

        #endregion
    }
}