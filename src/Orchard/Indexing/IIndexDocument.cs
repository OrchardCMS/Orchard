using System;
using System.Collections.Generic;

namespace Orchard.Indexing {

    public interface IIndexDocument {

        IIndexDocument SetContentItemId(int documentId);

        IIndexDocument Add(string name, string value);
        IIndexDocument Add(string name, DateTime value);
        IIndexDocument Add(string name, int value);
        IIndexDocument Add(string name, bool value);
        IIndexDocument Add(string name, float value);

        /// <summary>
        /// Whether to store the original value to the index
        /// </summary>
        IIndexDocument Store(bool store);

        /// <summary>
        /// Whether the content should be tokenized or not. If not, value will be taken as a whole
        /// </summary>
        IIndexDocument Analyze(bool analyze);

    }
}