using System.Collections.Generic;

namespace Orchard.Indexing.Services {
    public interface IIndexSynLock : ISingletonDependency {
        object GetSynLock(string indexName);
    }

    public class IndexSynLock : IIndexSynLock {
        private readonly Dictionary<string, object> _synLocks;
        private readonly object _synLock = new object();

        public IndexSynLock() {
            _synLocks =new Dictionary<string, object>();       
        }

        public object GetSynLock(string indexName) {
            lock(_synLock) {
                if(!_synLocks.ContainsKey(indexName)) {
                    _synLocks[indexName] = new object();
                }
                return _synLocks[indexName];
            }
        }
    }
}