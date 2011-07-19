using System;
using System.Collections.Generic;

namespace Orchard.Tokens {
    public class TokenTable {
        private readonly IDictionary<Tuple<string, string>, TokenDescriptor> _tokens;

        public TokenTable(IDictionary<Tuple<string, string>, TokenDescriptor> tokens) {
            _tokens = tokens;
        }

        public TokenDescriptor GetToken(string type, string name) {
            TokenDescriptor descriptor;
            var key = new Tuple<string, string>(type, name);
            _tokens.TryGetValue(key, out descriptor);
            return descriptor;
        }
    }
}
