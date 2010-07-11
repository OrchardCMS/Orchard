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
        private const string SearchIndexName = "Search";

        public IndexingBackgroundTask(
            IIndexNotifierHandler indexNotifierHandler) {
            _indexNotifierHandler = indexNotifierHandler;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            _indexNotifierHandler.UpdateIndex(SearchIndexName);
        }
    }
}
