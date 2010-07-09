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
            return NumericUtils.PrefixCodedToInt(_doc.GetField(name).StringValue());
        }

        public float GetFloat(string name) {
            return float.Parse(_doc.GetField(name).StringValue(), CultureInfo.InvariantCulture);
        }

        public bool GetBoolean(string name) {
            return bool.Parse(_doc.GetField(name).StringValue());
        }

        public string GetString(string name) {
            return _doc.GetField(name).StringValue();
        }

        public System.DateTime GetDateTime(string name) {
            return DateTools.StringToDate(_doc.GetField(name).StringValue());
        }
    }
}
