using System.Collections.Generic;
using System.Text;

namespace Orchard.ContentManagement {
    public class ContentIdentity {
        private readonly Dictionary<string, string> _dictionary;

        public ContentIdentity() {
            _dictionary = new Dictionary<string, string>();
        }

        public void Add(string name, string value) {
            if (_dictionary.ContainsKey(name)) {
                _dictionary[name] = value;
            }
            else {            
                _dictionary.Add(name, value);                
            }
        }

        public override string ToString() {
            var stringBuilder = new StringBuilder();
            foreach (var key in _dictionary.Keys) {
                stringBuilder.Append("/" + key + "=" + _dictionary[key]);
            }
            return stringBuilder.ToString();
        }
    }
}
