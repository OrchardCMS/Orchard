using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment;
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

        public void Install(Environment.Extensions.Models.Feature feature) {
        }

        public void Enable(Environment.Extensions.Models.Feature feature) {
            // create indexing tasks for all currently existing content, even when the module is enabled again
            // as some content might have been created while this module was not active, and indexing tasks 
            // would not exist for them, resulting in an uncomplete index.

            foreach (var contentItem in _contentManager.Query(VersionOptions.Published).List()) {
                _indexingTaskManager.CreateUpdateIndexTask(contentItem);
            }
        }

        public void Disable(Environment.Extensions.Models.Feature feature) {
        }

        public void Uninstall(Environment.Extensions.Models.Feature feature) {
        }
    }
}