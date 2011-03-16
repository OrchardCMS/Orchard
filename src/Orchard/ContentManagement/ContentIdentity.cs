using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement {
    public class ContentIdentity {
        private readonly Dictionary<string, string> _dictionary;

        public ContentIdentity() {
            _dictionary = new Dictionary<string, string>();
        }

        public ContentIdentity(string identity) {
            _dictionary = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(identity)) {
                var identityEntries = identity.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in identityEntries) {
                    var kv = token.Split(new[] {"="}, StringSplitOptions.None);
                    if (kv.Length == 2) {
                        _dictionary.Add(kv[0], kv[1]);
                    }
                }
            }
        }

        public void Add(string name, string value) {
            if (_dictionary.ContainsKey(name)) {
                _dictionary[name] = value;
            }
            else {            
                _dictionary.Add(name, value);                
            }
        }

        public string Get(string name) {
            return _dictionary.ContainsKey(name) ? _dictionary[name] : null;
        }

        public override string ToString() {
            var stringBuilder = new StringBuilder();
            foreach (var key in _dictionary.Keys) {
                stringBuilder.Append("/" + key + "=" + _dictionary[key]);
            }
            return stringBuilder.ToString();
        }

        public class ContentIdentityEqualityComparer : IEqualityComparer<ContentIdentity> {
            public bool Equals(ContentIdentity contentIdentity1, ContentIdentity contentIdentity2) {
                if (contentIdentity1._dictionary.Keys.Count != contentIdentity2._dictionary.Keys.Count)
                    return false;

                return contentIdentity1._dictionary.OrderBy(kvp => kvp.Key).SequenceEqual(contentIdentity2._dictionary.OrderBy(kvp => kvp.Key));
            }

            public int GetHashCode(ContentIdentity contentIdentity) {
                return contentIdentity.ToString().GetHashCode();
            }
        }

    }
}
