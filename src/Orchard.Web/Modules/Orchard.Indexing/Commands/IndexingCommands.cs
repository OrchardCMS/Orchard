using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Commands {
    public class IndexingCommands : DefaultOrchardCommandHandler {
        private readonly IEnumerable<IIndexNotifierHandler> _indexNotifierHandlers;
        private readonly IIndexManager _indexManager;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;
        private const string SearchIndexName = "Search";

        public IndexingCommands(
            IEnumerable<IIndexNotifierHandler> indexNotifierHandlers,
            IIndexManager indexManager,
            IIndexingTaskManager indexingTaskManager,
            IContentManager contentManager) {
            _indexNotifierHandlers = indexNotifierHandlers;
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;
            _indexManager = indexManager;
        }

        [OrchardSwitch]
        public string IndexName { get; set; }

        [OrchardSwitch]
        public string Query { get; set; }

        [OrchardSwitch]
        public string ContentItemId { get; set; }

        [CommandName("index update")]
        [CommandHelp("index update [/IndexName:<index name>]\r\n\t" + "Updates the index with the specified <index name>, or the search index if not specified")]
        [OrchardSwitches("IndexName")]
        public string Update() {
            if ( !_indexManager.HasIndexProvider() ) {
                return "No index available";
            }
            
            var indexName = String.IsNullOrWhiteSpace(IndexName) ? SearchIndexName : IndexName;
            foreach ( var handler in _indexNotifierHandlers ) {
                handler.UpdateIndex(indexName);
            }

            return "Index is now being updated...";
        }

        [CommandName("index rebuild")]
        [CommandHelp("index rebuild [/IndexName:<index name>]\r\n\t" + "Rebuilds the index with the specified <index name>, or the search index if not specified")]
        [OrchardSwitches("IndexName")]
        public string Rebuild() {
            if ( !_indexManager.HasIndexProvider() ) {
                return "No index available";
            }

            var indexName = String.IsNullOrWhiteSpace(IndexName) ? SearchIndexName : IndexName;
            var searchProvider = _indexManager.GetSearchIndexProvider();
            if ( searchProvider.Exists(indexName) )
                searchProvider.DeleteIndex(indexName);

            searchProvider.CreateIndex(indexName);
            return "Index is now being rebuilt...";
        }

        [CommandName("index search")]
        [CommandHelp("index search /Query:<query> [/IndexName:<index name>]\r\n\t" + "Searches the specified <query> terms in the index with the specified <index name>, or in the search index if not specified")]
        [OrchardSwitches("Query,IndexName")]
        public string Search() {
            if ( !_indexManager.HasIndexProvider() ) {
                return "No index available";
            }
            var indexName = String.IsNullOrWhiteSpace(IndexName) ? SearchIndexName : IndexName;
            var searchBuilder = _indexManager.GetSearchIndexProvider().CreateSearchBuilder(indexName);
            var results = searchBuilder.WithField("body", Query).WithField("title", Query).Search();

            Context.Output.WriteLine("{0} result{1}\r\n-----------------\r\n", results.Count(), results.Count() > 0 ? "s" : "");

            Context.Output.WriteLine("┌──────────────────────────────────────────────────────────────┬────────┐");
            Context.Output.WriteLine("│ {0} │ {1,6} │", "Title" + new string(' ', 60 - "Title".Length), "Score");
            Context.Output.WriteLine("├──────────────────────────────────────────────────────────────┼────────┤");
            foreach ( var searchHit in results ) {
                var title = searchHit.GetString("title") ?? "- no title -";
                title = title.Substring(0, Math.Min(60, title.Length));
                var score = searchHit.Score;
                Context.Output.WriteLine("│ {0} │ {1,6} │", title + new string(' ', 60 - title.Length), score);
            }
            Context.Output.WriteLine("└──────────────────────────────────────────────────────────────┴────────┘");

            Context.Output.WriteLine();
            return "End of search results";
        }

        [CommandName("index stats")]
        [CommandHelp("index stats [/IndexName:<index name>]\r\n\t" + "Displays some statistics about the index with the specified <index name>, or in the search index if not specified")]
        [OrchardSwitches("IndexName")]
        public string Stats() {
            if ( !_indexManager.HasIndexProvider() ) {
                return "No index available";
            }
            var indexName = String.IsNullOrWhiteSpace(IndexName) ? SearchIndexName : IndexName;
            Context.Output.WriteLine("Number of indexed documents: {0}", _indexManager.GetSearchIndexProvider().NumDocs(indexName));
            return "";
        }

        [CommandName("index refresh")]
        [CommandHelp("index refresh /ContenItem:<content item id> \r\n\t" + "Refreshes the index for the specifed <content item id>")]
        [OrchardSwitches("ContentItem")]
        public string Refresh() {
            int contenItemId;
            if ( !int.TryParse(ContentItemId, out contenItemId) ) {
                return "Invalid content item id. Not an integer.";
            }

            var contentItem = _contentManager.Get(contenItemId);
            _indexingTaskManager.CreateUpdateIndexTask(contentItem);

            return "Content Item marked for reindexing";
        }

        [CommandName("index delete")]
        [CommandHelp("index delete /ContenItem:<content item id>\r\n\t" + "Deletes the specifed <content item id> from the index")]
        [OrchardSwitches("ContentItem")]
        public string Delete() {
            int contenItemId;
            if(!int.TryParse(ContentItemId, out contenItemId)) {
                return "Invalid content item id. Not an integer.";
            }
            
            var contentItem = _contentManager.Get(contenItemId);
            _indexingTaskManager.CreateDeleteIndexTask(contentItem);

            return "Content Item marked for deletion";
        }

    }
}