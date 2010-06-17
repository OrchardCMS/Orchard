using System;
using System.Collections.Generic;

namespace Orchard.Indexing {

    public interface IDocumentIndex {

        IDocumentIndex SetContentItemId(int documentId);

        IDocumentIndex Add(string name, string value);
        IDocumentIndex Add(string name, string value, bool removeTags);
        IDocumentIndex Add(string name, DateTime value);
        IDocumentIndex Add(string name, int value);
        IDocumentIndex Add(string name, bool value);
        IDocumentIndex Add(string name, float value);

        /// <summary>
        /// Stores the original value to the index.
        /// </summary>
        IDocumentIndex Store();

        /// <summary>
        /// Content is analyzed and tokenized.
        /// </summary>
        IDocumentIndex Analyze();

        /// <summary>
        /// Whether some property have been added to this document, or otherwise if it's empty
        /// </summary>
        bool IsDirty { get; }

    }
}