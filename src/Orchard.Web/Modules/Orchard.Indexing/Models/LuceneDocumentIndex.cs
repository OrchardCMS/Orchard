using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Orchard.Utility.Extensions;

namespace Orchard.Indexing.Models {

    public class LuceneDocumentIndex : IDocumentIndex {

        public List<AbstractField> Fields { get; private set; }
        private AbstractField _previousField;

        public int Id { get; private set; }

        public LuceneDocumentIndex(int documentId) {
            Fields = new List<AbstractField>();
            SetContentItemId(documentId);
            IsDirty = false;
        }

        public bool IsDirty { get; private set; }

        public IDocumentIndex Add(string name, string value) {
            return Add(name, value, false);
        }

        public IDocumentIndex Add(string name, string value, bool removeTags) {
            AppendPreviousField();
            
            if(value == null) {
                value = String.Empty;
            }
            
            if(removeTags) {
                value = value.RemoveTags();
            }

            _previousField = new Field(name, value, Field.Store.NO, Field.Index.NOT_ANALYZED);
            IsDirty = true;
            return this;
        }

        public IDocumentIndex Add(string name, DateTime value) {
            return Add(name, DateTools.DateToString(value, DateTools.Resolution.SECOND));
        }

        public IDocumentIndex Add(string name, int value) {
            AppendPreviousField();
            _previousField = new NumericField(name, Field.Store.NO, true).SetIntValue(value);
            IsDirty = true;
            return this;
        }

        public IDocumentIndex Add(string name, bool value) {
            return Add(name, value.ToString());
        }

        public IDocumentIndex Add(string name, float value) {
            AppendPreviousField();
            _previousField = new NumericField(name, Field.Store.NO, true).SetFloatValue(value);
            IsDirty = true;
            return this;
        }

        public IDocumentIndex Add(string name, object value) {
            return Add(name, value.ToString());
        }

        public IDocumentIndex Store() {
            EnsurePreviousField();
            var index = _previousField.IsTokenized() ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED;
            _previousField = new Field(_previousField.Name(), _previousField.StringValue(), Field.Store.YES, index); 
            return this;
        }

        public IDocumentIndex Analyze() {
            EnsurePreviousField();

            var index = Field.Index.ANALYZED;
            var store = _previousField.IsStored() ? Field.Store.YES : Field.Store.NO;
            _previousField = new Field(_previousField.Name(), _previousField.StringValue(), store, index);
            return this;
        }

        public IDocumentIndex SetContentItemId(int id) {
            Id = id;
            Fields.Add(new Field("id", id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            return this;
        }

        private void AppendPreviousField() {
            if (_previousField == null) {
                return;
            }

            Fields.Add(_previousField);
            _previousField = null;
        }


        public void PrepareForIndexing() {
            AppendPreviousField();
        }

        private void EnsurePreviousField() {
            if(_previousField == null) {
                throw new ApplicationException("Operation can't be applied in this context.");
            }
        }
    }
}