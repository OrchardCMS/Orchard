using System;
using Lucene.Net.Documents;
using System.Globalization;
using Lucene.Net.Util;
using Orchard.Indexing;

namespace Lucene.Models {
    public class LuceneSearchHit : ISearchHit {
        private readonly Document _doc;
        private readonly float _score;

        public float Score { get { return _score; } }

        public LuceneSearchHit(Document document, float score) {
            _doc = document;
            _score = score;
        }

        public int ContentItemId { get { return int.Parse(GetString("id")); } }

        public int GetInt(string name) {
            var field = _doc.GetField(name);
            return field == null ? 0 : NumericUtils.PrefixCodedToInt(field.StringValue());
        }

        public float GetFloat(string name) {
            var field = _doc.GetField(name);
            return field == null ? 0 : float.Parse(field.StringValue(), CultureInfo.InvariantCulture);
        }

        public bool GetBoolean(string name) {
            var field = _doc.GetField(name);
            return field == null ? false : bool.Parse(field.StringValue());
        }

        public string GetString(string name) {
            var field = _doc.GetField(name);
            return field == null ? null : field.StringValue();
        }

        public DateTime GetDateTime(string name) {
            var field = _doc.GetField(name);
            return field == null ? DateTime.MinValue : DateTools.StringToDate(field.StringValue());
        }
    }
}
