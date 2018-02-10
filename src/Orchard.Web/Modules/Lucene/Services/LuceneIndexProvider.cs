using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Lucene.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Lucene.Services {
    /// <summary>
    /// Represents the default implementation of an IIndexProvider, based on Lucene
    /// </summary>
    public class LuceneIndexProvider : IIndexProvider {
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _basePath;
        private ILuceneAnalyzerProvider _analyzerProvider;

        public static readonly Version LuceneVersion = Version.LUCENE_29;
        public static readonly DateTime DefaultMinDateTime = new DateTime(1980, 1, 1);
        public static readonly int BatchSize = BooleanQuery.MaxClauseCount;

        public LuceneIndexProvider(
            IAppDataFolder appDataFolder, 
            ShellSettings shellSettings,
            ILuceneAnalyzerProvider analyzerProvider) {
            _appDataFolder = appDataFolder;
            _analyzerProvider = analyzerProvider;

            // TODO: (sebros) Find a common way to get where tenant's specific files should go. "Sites/Tenant" is hard coded in multiple places
            _basePath = _appDataFolder.Combine("Sites", shellSettings.Name, "Indexes");

            // Ensures the directory exists
            EnsureDirectoryExists();

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private void EnsureDirectoryExists() {
            var directory = new DirectoryInfo(_appDataFolder.MapPath(_basePath));
            if(!directory.Exists) {
                directory.Create();
            }
        }
        
        protected virtual Directory GetDirectory(string indexName) {
            var directoryInfo = new DirectoryInfo(_appDataFolder.MapPath(_appDataFolder.Combine(_basePath, indexName)));
            return FSDirectory.Open(directoryInfo);
        }

        private static Document CreateDocument(LuceneDocumentIndex indexDocument) {
            var doc = new Document();

            indexDocument.PrepareForIndexing();
            foreach(var field in indexDocument.Fields) {
                doc.Add(field);
            }
            return doc;
        }

        public bool Exists(string indexName) {
            return new DirectoryInfo(_appDataFolder.MapPath(_appDataFolder.Combine(_basePath, indexName))).Exists;
        }

        public IEnumerable<string> List() {
            return _appDataFolder.ListDirectories(_basePath).Select(Path.GetFileNameWithoutExtension);
        }

        public bool IsEmpty(string indexName) {
            if ( !Exists(indexName) ) {
                return true;
            }

            using (var reader = IndexReader.Open(GetDirectory(indexName), true)) {
                return reader.NumDocs() == 0;
            }
        }

        public int NumDocs(string indexName) {
            if ( !Exists(indexName) ) {
                return 0;
            }

            using (var reader = IndexReader.Open(GetDirectory(indexName), true)) {
                return reader.NumDocs();
            }
        }

        public void CreateIndex(string indexName) {
            using (new IndexWriter(GetDirectory(indexName), _analyzerProvider.GetAnalyzer(indexName), true, IndexWriter.MaxFieldLength.UNLIMITED)) {
            }
        }

        public void DeleteIndex(string indexName) {
            new DirectoryInfo(_appDataFolder.MapPath(_appDataFolder.Combine(_basePath, indexName)))
                .Delete(true);
        }

        public void Store(string indexName, IDocumentIndex indexDocument) {
            Store(indexName, new [] { (LuceneDocumentIndex)indexDocument });
        }

        public void Store(string indexName, IEnumerable<IDocumentIndex> indexDocuments) {
            Store(indexName, indexDocuments.Cast<LuceneDocumentIndex>());
        }

        public void Store(string indexName, IEnumerable<LuceneDocumentIndex> indexDocuments) {
            indexDocuments = indexDocuments.ToArray();

            if (!indexDocuments.Any()) {
                return;
            }

            // Remove any previous document for these content items
            Delete(indexName, indexDocuments.Select(i => i.ContentItemId));

            using (var writer = new IndexWriter(GetDirectory(indexName), _analyzerProvider.GetAnalyzer(indexName), false, IndexWriter.MaxFieldLength.UNLIMITED)) {
                foreach (var indexDocument in indexDocuments) {
                    var doc = CreateDocument(indexDocument);

                    writer.AddDocument(doc);
                    Logger.Debug("Document [{0}] indexed", indexDocument.ContentItemId);
                }
            }
        }

        public void Delete(string indexName, int documentId) {
            Delete(indexName, new[] { documentId });
        }

        public void Delete(string indexName, IEnumerable<int> documentIds) {
            documentIds = documentIds.ToArray();
            
            if (!documentIds.Any()) {
                return;
            }

            using (var writer = new IndexWriter(GetDirectory(indexName), _analyzerProvider.GetAnalyzer(indexName), false, IndexWriter.MaxFieldLength.UNLIMITED)) {
                // Process documents by batch as there is a max number of terms a query can contain (1024 by default).
                var pageCount = documentIds.Count() / BatchSize + 1;
                for (int page = 0; page < pageCount; page++) {
                    var query = new BooleanQuery();

                    try {
                        var batch = documentIds
                            .Skip(page * BatchSize)
                            .Take(BatchSize);

                        foreach (var id in batch) {
                            query.Add(new BooleanClause(new TermQuery(new Term("id", id.ToString(CultureInfo.InvariantCulture))), Occur.SHOULD));
                        }

                        writer.DeleteDocuments(query);
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "An unexpected error occurred while removing the documents [{0}] from the index [{1}].", String.Join(", ", documentIds), indexName);
                    }
                }
            }
        }

        public IDocumentIndex New(int documentId) {
            return new LuceneDocumentIndex(documentId, T);
        }

        public ISearchBuilder CreateSearchBuilder(string indexName) {
            return new LuceneSearchBuilder(GetDirectory(indexName), _analyzerProvider, indexName) { Logger = Logger };
        }

        public IEnumerable<string> GetFields(string indexName) {
            if ( !Exists(indexName) ) {
                return Enumerable.Empty<string>();
            }

            using(var reader = IndexReader.Open(GetDirectory(indexName), true)) {
                return reader.GetFieldNames(IndexReader.FieldOption.ALL).ToList();
            }
        }
    }
}
