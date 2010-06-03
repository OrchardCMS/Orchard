using System;
using Orchard.ContentManagement;

namespace Orchard.Tasks.Indexing {
    public interface IIndexingTask {
        ContentItem ContentItem { get; }
        DateTime? CreatedUtc { get; }
    }
}
