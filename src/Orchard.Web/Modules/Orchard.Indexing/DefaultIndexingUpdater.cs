using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Tasks.Indexing;
using Orchard.ContentManagement;

namespace Orchard.Indexing {
    public class DefaultIndexingUpdater : IFeatureEventHandler {
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;

        public DefaultIndexingUpdater (IIndexingTaskManager indexingTaskManager, IContentManager contentManager){
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;
        }

        public void Installing(Feature feature) {
        }

        public void Installed(Feature feature) {
        }

        public void Enabling(Feature feature) {
        }

        public void Enabled(Feature feature) {
            // create indexing tasks for all currently existing content, even when the module is enabled again
            // as some content might have been created while this module was not active, and indexing tasks 
            // would not exist for them, resulting in an uncomplete index.

            foreach (var contentItem in _contentManager.Query(VersionOptions.Published).List()) {
                _indexingTaskManager.CreateUpdateIndexTask(contentItem);
            }
        }

        public void Disabling(Feature feature) {
        }

        public void Disabled(Feature feature) {
        }

        public void Uninstalling(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
        }
    }
}