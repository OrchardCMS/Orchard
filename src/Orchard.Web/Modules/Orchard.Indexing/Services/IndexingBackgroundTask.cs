using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Tasks;

namespace Orchard.Indexing.Services {
    /// <summary>
    /// Regularly fires IIndexNotifierHandler events
    /// </summary>
    [UsedImplicitly]
    public class IndexingBackgroundTask : IBackgroundTask {
        private readonly IIndexNotifierHandler _indexNotifierHandler;
        private readonly IIndexManager _indexManager;

        public IndexingBackgroundTask(
            IIndexNotifierHandler indexNotifierHandler,
            IIndexManager indexManager) {
            _indexNotifierHandler = indexNotifierHandler;
            _indexManager = indexManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            if (!_indexManager.HasIndexProvider()) {
                return;
            }

            foreach (var index in _indexManager.GetSearchIndexProvider().List()) {
                _indexNotifierHandler.UpdateIndex(index);
            }
        }
    }
}
