using System;
using System.Collections.Generic;

namespace Orchard.Tokens {
    public class TokenBuilder {
        private readonly IDictionary<Tuple<string, string>, TokenDescriptor> _descriptors = new Dictionary<Tuple<string, string>, TokenDescriptor>(new TokenTypeAndNameEqualityComparer());

        public TokenDescriptor<object> Describe(string tokenType, string tokenName, Func<object, object> tokenValue) {
            var descriptor = new TokenDescriptor<object> { Type = tokenType, Name = tokenName }.WithValue(tokenValue);
            _descriptors[new Tuple<string, string>(tokenType, tokenName)] = descriptor;
            return descriptor;
        }

        public TokenDescriptor<T> Describe<T>(string tokenType, string tokenName, Func<T, object> tokenValue) {
            var descriptor = new TokenDescriptor<T> { Type = tokenType, Name = tokenName }.WithValue(tokenValue);
            _descriptors[new Tuple<string, string>(tokenType, tokenName)] = descriptor;
            return descriptor;
        }

        public TokenDescriptor<T> Describe<T>(string tokenType, string tokenName) {
            var descriptor = new TokenDescriptor<T> { Type = tokenType, Name = tokenName };
            _descriptors[new Tuple<string, string>(tokenType, tokenName)] = descriptor;
            return descriptor;
        }

        public virtual TokenTable BuildTokens() {
            return new TokenTable(_descriptors);
        }

        private class TokenTypeAndNameEqualityComparer : IEqualityComparer<Tuple<string, string>> {
            public bool Equals(Tuple<string, string> x, Tuple<string, string> y) {
                // todo: what does it do with nulls?
                return string.Equals(x.Item1, y.Item1, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(Tuple<string, string> obj) {
                return (obj.Item1 == null ? 0 : obj.Item1.ToLowerInvariant().GetHashCode()) ^ (obj.Item2 == null ? 0 : obj.Item2.ToLowerInvariant().GetHashCode());
            }
        }
    }
}
