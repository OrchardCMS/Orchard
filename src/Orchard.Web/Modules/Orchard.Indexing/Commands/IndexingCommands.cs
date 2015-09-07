using System;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Indexing.Services;
using Orchard.Tasks.Indexing;
using Orchard.Utility.Extensions;

namespace Orchard.Indexing.Commands {
    public class IndexingCommands : DefaultOrchardCommandHandler {
        private readonly IIndexManager _indexManager;
        private readonly IIndexingService _indexingService;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;

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

        [CommandName("index create")]
        [CommandHelp("index create <index>\r\n\t" + "Creates a new index with the specified name")]
        public void Create(string index) {
            if (!_indexManager.HasIndexProvider()) {
                Context.Output.WriteLine(T("No index service available"));
                return;
            }

            if (string.IsNullOrWhiteSpace(index)) {
                Context.Output.WriteLine(T("Invalid index name."));
                return;
            }

            if (index.ToSafeName() != index) {
                Context.Output.WriteLine(T("Invalid index name."));
            }
            else {
                var indexProvider = _indexManager.GetSearchIndexProvider();
                if(indexProvider == null) {
                    Context.Output.WriteLine(T("New indexing service was found. Please enable a module like Lucene."));
                }
                else {
                    if (indexProvider.Exists(index)) {
                        Context.Output.WriteLine(T("The specified index already exists."));
                    }
                    else {
                        _indexManager.GetSearchIndexProvider().CreateIndex(index);
                        Context.Output.WriteLine(T("New index has been created successfully."));
                    }
                }
            }
        }

        [CommandName("index update")]
        [CommandHelp("index update <index>\r\n\t" + "Updates the specified index")]
        public void Update(string index) {
            if (string.IsNullOrWhiteSpace(index)) {
                Context.Output.WriteLine(T("Invalid index name."));
                return;
            }

            _indexingService.UpdateIndex(index);
            Context.Output.WriteLine(T("Index is now being updated..."));
        }

        [CommandName("index rebuild")]
        [CommandHelp("index rebuild <index> \r\n\t" + "Rebuilds the specified index")]
        public void Rebuild(string index) {
            if (string.IsNullOrWhiteSpace(index)) {
                Context.Output.WriteLine(T("Invalid index name."));
                return;
            }

            _indexingService.RebuildIndex(index);
            Context.Output.WriteLine(T("Index is now being rebuilt..."));
        }

        [CommandName("index query")]
        [CommandHelp("index query <index> /Query:<query>\r\n\t" + "Searches the specified <query> terms in the specified index")]
        [OrchardSwitches("Query")]
        public void Search(string index) {
            if (string.IsNullOrWhiteSpace(index)) {
                Context.Output.WriteLine(T("Invalid index name."));
                return;
            }

            if ( !_indexManager.HasIndexProvider() ) {
                Context.Output.WriteLine(T("No index available"));
                return;
            }
            var searchBuilder = _indexManager.GetSearchIndexProvider().CreateSearchBuilder(index);
            var results = searchBuilder.Parse( new [] {"body", "title"}, Query).Search();

            Context.Output.WriteLine("{0} result{1}\r\n-----------------\r\n", results.Count(), results.Count() > 0 ? "s" : "");

            Context.Output.WriteLine("┌──────────────────────────────────────────────────────────────┬────────┐");
            Context.Output.WriteLine("│ {0} │ {1,6} │", "Title" + new string(' ', 60 - "Title".Length), "Score");
            Context.Output.WriteLine("├──────────────────────────────────────────────────────────────┼────────┤");
            foreach ( var searchHit in results ) {
                var contentItem = _contentManager.Get(searchHit.ContentItemId);
                var metadata = _contentManager.GetItemMetadata(contentItem);
                var title = String.IsNullOrWhiteSpace(metadata.DisplayText) ? "- no title -" : metadata.DisplayText;
                title = title.Substring(0, Math.Min(60, title.Length));
                var score = Math.Round(searchHit.Score, 2).ToString();
                Context.Output.WriteLine("│ {0} │ {1,6} │", title + new string(' ', 60 - title.Length), score);
            }
            Context.Output.WriteLine("└──────────────────────────────────────────────────────────────┴────────┘");

            Context.Output.WriteLine();
            Context.Output.WriteLine(T("End of search results"));
        }

        [CommandName("index stats")]
        [CommandHelp("index stats <index>\r\n\t" + "Displays some statistics about the search index")]
        [OrchardSwitches("IndexName")]
        public void Stats(string index) {
            if (string.IsNullOrWhiteSpace(index)) {
                Context.Output.WriteLine(T("Invalid index name."));
                return;
            }

            if ( !_indexManager.HasIndexProvider() ) {
                Context.Output.WriteLine(T("No index available"));
                return;
            }

            Context.Output.WriteLine(T("Number of indexed documents: {0}", _indexManager.GetSearchIndexProvider().NumDocs(index)));
        }

        [CommandName("index refresh")]
        [CommandHelp("index refresh /ContentItem:<content item id> \r\n\t" + "Refreshes the index for the specifed <content item id>")]
        [OrchardSwitches("ContentItem")]
        public void Refresh() {
            int contentItemId;
            if ( !int.TryParse(ContentItem, out contentItemId) ) {
                Context.Output.WriteLine(T("Invalid content item id. Not an integer."));
                return;
            }

            var contentItem = _contentManager.Get(contentItemId);
            _indexingTaskManager.CreateUpdateIndexTask(contentItem);

            Context.Output.WriteLine(T("Content Item marked for reindexing"));
        }

        [CommandName("index delete")]
        [CommandHelp("index delete /ContentItem:<content item id>\r\n\t" + "Deletes the specifed <content item id> from the index")]
        [OrchardSwitches("ContentItem")]
        public void Delete() {
            int contentItemId;
            if(!int.TryParse(ContentItem, out contentItemId)) {
                Context.Output.WriteLine(T("Invalid content item id. Not an integer."));
                return;
            }
            
            var contentItem = _contentManager.Get(contentItemId);
            _indexingTaskManager.CreateDeleteIndexTask(contentItem);

            Context.Output.WriteLine(T("Content Item marked for deletion"));
        }

    }
}