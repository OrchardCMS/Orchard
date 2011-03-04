using System;
using System.Collections.Generic;
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
        private readonly ShellSettings _shellSettings;
        public static readonly Version LuceneVersion = Version.LUCENE_29;
        private readonly Analyzer _analyzer ;
        private readonly string _basePath;
        public static readonly DateTime DefaultMinDateTime = new DateTime(1980, 1, 1);

        public LuceneIndexProvider(IAppDataFolder appDataFolder, ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
            _analyzer = CreateAnalyzer();

            // TODO: (sebros) Find a common way to get where tenant's specific files should go. "Sites/Tenant" is hard coded in multiple places
            _basePath = _appDataFolder.Combine("Sites", _shellSettings.Name, "Indexes");

            Logger = NullLogger.Instance;

            // Ensures the directory exists
            EnsureDirectoryExists();

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public static Analyzer CreateAnalyzer() {
            // StandardAnalyzer does lower-case and stop-word filtering. It also removes punctuation
            return new StandardAnalyzer(LuceneVersion);
        }

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

        public bool IsEmpty(string indexName) {
            if ( !Exists(indexName) ) {
                return true;
            }

            var reader = IndexReader.Open(GetDirectory(indexName), true);

            try {
                return reader.NumDocs() == 0;
            }
            finally {
                reader.Close();
            }
        }

        public int NumDocs(string indexName) {
            if ( !Exists(indexName) ) {
                return 0;
            }

            var reader = IndexReader.Open(GetDirectory(indexName), true);

            try {
                return reader.NumDocs();
            }
            finally {
                reader.Close();
            }
        }

        public void CreateIndex(string indexName) {
            var writer = new IndexWriter(GetDirectory(indexName), _analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            writer.Close();

            Logger.Information("Index [{0}] created", indexName);
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
            if (indexDocuments.AsQueryable().Count() == 0) {
                return;
            }

            // Remove any previous document for these content items
            Delete(indexName, indexDocuments.Select(i => i.ContentItemId));

            var writer = new IndexWriter(GetDirectory(indexName), _analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
            LuceneDocumentIndex current = null;

            try {

                foreach (var indexDocument in indexDocuments) {
                    current = indexDocument;
                    var doc = CreateDocument(indexDocument);

                    writer.AddDocument(doc);
                    Logger.Debug("Document [{0}] indexed", indexDocument.ContentItemId);
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "An unexpected error occured while add the document [{0}] from the index [{1}].", current.ContentItemId, indexName);
            }
            finally {
                writer.Optimize();
                writer.Close();
            }
        }

        public void Delete(string indexName, int documentId) {
            Delete(indexName, new[] { documentId });
        }

        public void Delete(string indexName, IEnumerable<int> documentIds) {
            if (!documentIds.Any()) {
                return;
            }

            var writer = new IndexWriter(GetDirectory(indexName), _analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);

            try {
                var query = new BooleanQuery();

                try {
                    foreach (var id in documentIds) {
                        query.Add(new BooleanClause(new TermQuery(new Term("id", id.ToString())), BooleanClause.Occur.SHOULD));
                    }

                    writer.DeleteDocuments(query);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "An unexpected error occured while removing the documents [{0}] from the index [{1}].", String.Join(", ", documentIds), indexName);
                }
            }
            finally {
                writer.Close();
            }
        }

        public IDocumentIndex New(int documentId) {
            return new LuceneDocumentIndex(documentId, T);
        }

        public ISearchBuilder CreateSearchBuilder(string indexName) {
            return new LuceneSearchBuilder(GetDirectory(indexName)) { Logger = Logger };
        }

        public IEnumerable<string> GetFields(string indexName) {
            if ( !Exists(indexName) ) {
                return Enumerable.Empty<string>();
            }

            var reader = IndexReader.Open(GetDirectory(indexName), true);

            try {
                return reader.GetFieldNames(IndexReader.FieldOption.ALL).ToList();
            }
            finally {
                reader.Close();
            }
        }
    }
}
