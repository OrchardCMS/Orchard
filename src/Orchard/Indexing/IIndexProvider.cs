namespace Orchard.Indexing {
    public interface IIndexProvider : IDependency {
        /// <summary>
        /// Creates a new index
        /// </summary>
        void CreateIndex(string name);

        /// <summary>
        /// Checks whether an index is already existing or not
        /// </summary>
        bool Exists(string name);

        /// <summary>
        /// Deletes an existing index
        /// </summary>
        void DeleteIndex(string name);

        /// <summary>
        /// Loads an existing document 
        /// </summary>
        IIndexDocument Get(string indexName, int documentId);

        /// <summary>
        /// Creates an empty document
        /// </summary>
        /// <returns></returns>
        IIndexDocument New(int documentId);

        /// <summary>
        /// Adds a new document to the index
        /// </summary>
        void Store(string indexName, IIndexDocument indexDocument);

        /// <summary>
        /// Removes an existing document from the index
        /// </summary>
        void Delete(string indexName, int id);

        /// <summary>
        /// Creates a search builder for this provider
        /// </summary>
        /// <returns>A search builder instance</returns>
        ISearchBuilder CreateSearchBuilder(string indexName);
    }
}