using Orchard.Events;

namespace Orchard.Indexing.Services {
    /// <summary>
    /// Manages the creation of indexing tasks in batches.
    /// </summary>
    public interface ICreateUpdateIndexTaskService : IEventHandler {
        /// <summary>
        /// Creates the next set of indexing task batches, and renews itself with the next batch.
        /// </summary>
        /// <param name="contentTypeName">The content type name.</param>
        /// <param name="currentBatchIndex">The current batch index. This must be string, because <see cref="DefaultOrchardEventBus"/> throws InvalidCastException if this is int.</param>
        void CreateNextIndexingTaskBatch(string contentTypeName, string currentBatchIndex);
    }
}