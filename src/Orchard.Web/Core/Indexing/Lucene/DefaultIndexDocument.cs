using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Lucene.Net.Documents;
using Orchard.Indexing;
using Orchard.Mvc.Html;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Indexing.Lucene {

    public class DefaultIndexDocument : IIndexDocument {

        public List<AbstractField> Fields { get; private set; }
        private AbstractField _previousField;
        public int Id { get; private set; }

        public DefaultIndexDocument(int documentId) {
            Fields = new List<AbstractField>();
            SetContentItemId(documentId);
        }

        public IIndexDocument Add(string name, string value) {
            return Add(name, value, false);
        }

        public IIndexDocument Add(string name, string value, bool removeTags) {
            AppendPreviousField();
            
            if(value == null) {
                value = String.Empty;
            }
            
            if(removeTags) {
                value = value.RemoveTags();
            }

            _previousField = new Field(name, value, Field.Store.YES, Field.Index.ANALYZED);
            return this;
        }

        public IIndexDocument Add(string name, DateTime value) {
            AppendPreviousField();
            _previousField = new Field(name, DateTools.DateToString(value, DateTools.Resolution.SECOND), Field.Store.YES, Field.Index.NOT_ANALYZED);
            return this;
        }

        public IIndexDocument Add(string name, int value) {
            AppendPreviousField();
            _previousField = new NumericField(name, Field.Store.YES, true).SetIntValue(value);
            return this;
        }

        public IIndexDocument Add(string name, bool value) {
            AppendPreviousField();
            _previousField = new Field(name, value.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED);
            return this;
        }

        public IIndexDocument Add(string name, float value) {
            AppendPreviousField();
            _previousField = new NumericField(name, Field.Store.YES, true).SetFloatValue(value);
            return this;
        }

        public IIndexDocument Add(string name, object value) {
            AppendPreviousField();
            _previousField = new Field(name, value.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED);
            return this;
        }

        public IIndexDocument Store(bool store) {
            EnsurePreviousField();
            if(store != _previousField.IsStored()) {
                var index = _previousField.IsTokenized() ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED;
                _previousField = new Field(_previousField.Name(), _previousField.StringValue(), store ? Field.Store.YES : Field.Store.NO, index); 
            }
            return this;
        }

        public IIndexDocument Analyze(bool analyze) {
            EnsurePreviousField();
            if (_previousField.IsTokenized() == analyze) {
                return this;
            }

            var index = analyze ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED;
            var store = _previousField.IsStored() ? Field.Store.YES : Field.Store.NO;
            _previousField = new Field(_previousField.Name(), _previousField.StringValue(), store, index);
            return this;
        }

        public IIndexDocument SetContentItemId(int id) {
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