using System;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Indexing.Services;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Commands {
    public class IndexingCommands : DefaultOrchardCommandHandler {
        private readonly IIndexManager _indexManager;
        private readonly IIndexingService _indexingService;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;
        private const string SearchIndexName = "Search";

        public IndexingCommands(
            IIndexManager indexManager,
            IIndexingService indexingService,
            IIndexingTaskManager indexingTaskManager,
            IContentManager contentManager) {
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;
            _indexManager = indexManager;
            _indexingService = indexingService;
        }

        [OrchardSwitch]
        public string Query { get; set; }

        [OrchardSwitch]
        public string ContentItem { get; set; }

        [CommandName("index update")]
        [CommandHelp("index update\r\n\t" + "Updates the search index")]
        public string Update() {
            _indexingService.UpdateIndex(SearchIndexName);

            return T("Index is now being updated...").Text;
        }

        [CommandName("index rebuild")]
        [CommandHelp("index rebuild \r\n\t" + "Rebuilds the search index")]
        public string Rebuild() {
            _indexingService.RebuildIndex(SearchIndexName);

            return T("Index is now being rebuilt...").Text;
        }

        [CommandName("index search")]
        [CommandHelp("index search /Query:<query>\r\n\t" + "Searches the specified <query> terms in the search index")]
        [OrchardSwitches("Query")]
        public string Search() {
            if ( !_indexManager.HasIndexProvider() ) {
                throw new OrchardException(T("No index available"));
            }
            var searchBuilder = _indexManager.GetSearchIndexProvider().CreateSearchBuilder(SearchIndexName);
            var results = searchBuilder.Parse( new [] {"body", "title"}, Query).Search();

            Context.Output.WriteLine("{0} result{1}\r\n-----------------\r\n", results.Count(), results.Count() > 0 ? "s" : "");

            Context.Output.WriteLine("┌──────────────────────────────────────────────────────────────┬────────┐");
            Context.Output.WriteLine("│ {0} │ {1,6} │", "Title" + new string(' ', 60 - "Title".Length), "Score");
            Context.Output.WriteLine("├──────────────────────────────────────────────────────────────┼────────┤");
            foreach ( var searchHit in results ) {
                var contentItem = _contentManager.Get(searchHit.ContentItemId);
                var routable = contentItem.As<IRoutableAspect>();
                var title = routable == null ? "- no title -" : routable.Title;
                title = title.Substring(0, Math.Min(60, title.Length));
                var score = Math.Round(searchHit.Score, 2).ToString();
                Context.Output.WriteLine("│ {0} │ {1,6} │", title + new string(' ', 60 - title.Length), score);
            }
            Context.Output.WriteLine("└──────────────────────────────────────────────────────────────┴────────┘");

            Context.Output.WriteLine();
            return T("End of search results").Text;
        }

        [CommandName("index stats")]
        [CommandHelp("index stats\r\n\t" + "Displays some statistics about the search index")]
        [OrchardSwitches("IndexName")]
        public string Stats() {
            if ( !_indexManager.HasIndexProvider() ) {
                throw new OrchardException(T("No index available"));
            }

            return T("Number of indexed documents: {0}", _indexManager.GetSearchIndexProvider().NumDocs(SearchIndexName)).Text;
        }

        [CommandName("index refresh")]
        [CommandHelp("index refresh /ContentItem:<content item id> \r\n\t" + "Refreshes the index for the specifed <content item id>")]
        [OrchardSwitches("ContentItem")]
        public string Refresh() {
            int contentItemId;
            if ( !int.TryParse(ContentItem, out contentItemId) ) {
                throw new OrchardException(T("Invalid content item id. Not an integer."));
            }

            var contentItem = _contentManager.Get(contentItemId);
            _indexingTaskManager.CreateUpdateIndexTask(contentItem);

            return T("Content Item marked for reindexing").Text;
        }

        [CommandName("index delete")]
        [CommandHelp("index delete /ContentItem:<content item id>\r\n\t" + "Deletes the specifed <content item id> from the index")]
        [OrchardSwitches("ContentItem")]
        public string Delete() {
            int contentItemId;
            if(!int.TryParse(ContentItem, out contentItemId)) {
                throw new OrchardException(T("Invalid content item id. Not an integer."));
            }
            
            var contentItem = _contentManager.Get(contentItemId);
            _indexingTaskManager.CreateDeleteIndexTask(contentItem);

            return T("Content Item marked for deletion").Text;
        }

    }
}