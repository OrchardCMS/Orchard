using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Indexing;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;
using Orchard.Logging;

namespace Orchard.Core.Indexing.Lucene {
    /// <summary>
    /// Represents the default implementation of an IIndexProvider based on Lucene
    /// </summary>
    public class DefaultIndexProvider : IIndexProvider {
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        public static readonly Version LuceneVersion = Version.LUCENE_29;
        private readonly Analyzer _analyzer = new StandardAnalyzer(LuceneVersion);
        private readonly string _basePath;

        public ILogger Logger { get; set; }

        public DefaultIndexProvider(IAppDataFolder appDataFolder, ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;

            // TODO: (sebros) Find a common way to get where tenant's specific files should go. "Sites/Tenant" is hard coded in multiple places
            _basePath = Path.Combine("Sites", _shellSettings.Name, "Indexes");

            Logger = NullLogger.Instance;

            // Ensures the directory exists
            var directory = new DirectoryInfo(_appDataFolder.MapPath(_basePath));
            if(!directory.Exists) {
                directory.Create();
            }
        }

        protected virtual Directory GetDirectory(string indexName) {
            var directoryInfo = new DirectoryInfo(_appDataFolder.MapPath(Path.Combine(_basePath, indexName)));
            return FSDirectory.Open(directoryInfo);
        }

        private static Document CreateDocument(DefaultIndexDocument indexDocument) {
            var doc = new Document();

            indexDocument.PrepareForIndexing();
            foreach(var field in indexDocument.Fields) {
                doc.Add(field);
            }
            return doc;
        }

        public bool Exists(string indexName) {
            return new DirectoryInfo(_appDataFolder.MapPath(Path.Combine(_basePath, indexName))).Exists;
        }

        public void CreateIndex(string indexName) {
            var writer = new IndexWriter(GetDirectory(indexName), _analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            writer.Close();

            Logger.Information("Index [{0}] created", indexName);
        }

        public void DeleteIndex(string indexName) {
            new DirectoryInfo(Path.Combine(_appDataFolder.MapPath(Path.Combine(_basePath, indexName))))
                .Delete(true);
        }

        public void Store(string indexName, IIndexDocument indexDocument) {
            Store(indexName, (DefaultIndexDocument)indexDocument);
        }

        public void Store(string indexName, DefaultIndexDocument indexDocument) {
            var writer = new IndexWriter(GetDirectory(indexName), _analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);

            try {
                var doc = CreateDocument(indexDocument);
                writer.AddDocument(doc);
                Logger.Debug("Document [{0}] indexed", indexDocument.Id);
            }
            catch ( Exception ex ) {
                Logger.Error(ex, "An unexpected error occured while removing the document [{0}] from the index [{1}].", indexDocument.Id, indexName);
            }
            finally {
                writer.Close();
            }

        }

        public void Delete(string indexName, int id) {
			var reader = IndexReader.Open(GetDirectory(indexName), false);

            try {
                var term = new Term("id", id.ToString());
                if ( reader.DeleteDocuments(term) != 0 ) {
                    Logger.Error("The document [{0}] could not be removed from the index [{1}]", id, indexName);
                }
                else {
                    Logger.Debug("Document [{0}] removed from index", id);
                }
            }
            catch ( Exception ex ) {
                Logger.Error(ex, "An unexpected error occured while removing the document [{0}] from the index [{1}].", id, indexName);
            }
            finally {
                reader.Close();
            }
        }

        public IIndexDocument New(int documentId) {
            return new DefaultIndexDocument(documentId);
        }

        public ISearchBuilder CreateSearchBuilder(string indexName) {
            return new DefaultSearchBuilder(GetDirectory(indexName));
        }

        public IIndexDocument Get(string indexName, int id) {
            throw new NotImplementedException();
        }
    }
}
