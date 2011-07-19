using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace Orchard.Tokens {
    public class DefaultTokenizer : ITokenizer {
        private readonly ITokenManager _tokenManager;

        public DefaultTokenizer(ITokenManager tokenManager) {
            _tokenManager = tokenManager;
        }

        public IEnumerable<TokenContext> ParseTokens(string str, object contexts) {
            return ParseTokens(str, new RouteValueDictionary(contexts));
        }

        public IEnumerable<TokenContext> ParseTokens(string str, IDictionary<string, object> contexts) {
            if (!string.IsNullOrEmpty(str)) {
                var inToken = false;
                var tokenStart = 0;
                for (var i = 0; i < str.Length; i++) {
                    var c = str[i];

                    if (inToken) {
                        if (c == '}' && (i + 1 >= str.Length || str[i + 1] != '}')) {
                            inToken = false;
                            var length = i - tokenStart + 1;
                            var token = str.Substring(tokenStart, length);
                            var tokenContext = ParseToken(token, contexts);
                            tokenContext.Offset = tokenStart;
                            tokenContext.Length = length;
                            yield return tokenContext;
                        }
                    }
                    else {
                        if (c == '{') {
                            if (i + 1 < str.Length && str[i + 1] == '{') {
                                yield return new TokenContext { Replacement = "{", Offset = i, Length = 2 };
                                i++;
                            }
                            else {
                                inToken = true;
                                tokenStart = i;
                            }
                        }
                        else if (c == '}' && i + 1 < str.Length && str[i + 1] == '}') {
                            yield return new TokenContext { Replacement = "}", Offset = i, Length = 2 };
                            i++;
                        }
                    }
                }
            }
        }

        private TokenContext ParseToken(string token, IDictionary<string, object> contexts) {
            var tokenContext = new TokenContext();
            // strip delimiters
            token = token.Substring(1, token.Length - 2);
            // split on tokenparts:format
            var tokenAreas = token.Split(':');
            token = tokenAreas[0];
            var format = tokenAreas.Length == 2 ? tokenAreas[1] : null;
            // enumerate token parts (dotted syntax)
            var tokenParts = token.Split('.');
            if (tokenParts.Length < 2) {
                return tokenContext;
            }
            var tokenType = tokenParts[0].Trim();
            var tokenTable = _tokenManager.GetTokenTable();
            var readingProperties = false;
            object tokenValue = null;
            if (contexts != null) {
                contexts.TryGetValue(tokenType, out tokenValue);
                if (tokenValue == null && !"Site".Equals(tokenType, StringComparison.OrdinalIgnoreCase)) {
                    // a token using a context that was not provided (and is not 'Site' global context)
                    tokenContext.Replacement = null;
                    tokenContext.Token = null;
                    return tokenContext;
                }
            }
            foreach (var tokenPart in tokenParts.Skip(1)) {
                var tokenName = tokenPart.Trim();
                var tokenDescriptor = readingProperties ? null : tokenTable.GetToken(tokenType, tokenName);
                if (tokenDescriptor == null) {
                    // token does not exist for the given token type
                    // try a property of the running value
                    if (tokenValue != null) {
                        if (!GetProperty(tokenValue, tokenName, out tokenValue)) {
                            // token not found, property not found.
                            tokenContext.Replacement = null;
                            tokenContext.Token = null;
                            return tokenContext;
                        }
                        // once we have to read a value via property,
                        // we will never know what the tokenType is. All sub
                        // tokens from here on will need to be properties.
                        readingProperties = true;
                        tokenType = null;
                        tokenContext.Replacement = tokenValue;
                        tokenContext.Token = null;
                    }
                    else {
                        // token not found, and can't do a property reference because
                        // the running value is null.
                        tokenContext.Replacement = null;
                        tokenContext.Token = null;
                        return tokenContext;
                    }
                }
                else {
                    var context = tokenValue;
                    if (context == null && contexts != null) {
                        contexts.TryGetValue(tokenType, out context);
                        if (context == null && !"Site".Equals(tokenType, StringComparison.OrdinalIgnoreCase)) {
                            // a token using a context that was not provided
                            tokenContext.Replacement = null;
                            tokenContext.Token = null;
                            return tokenContext;
                        }
                    }
                    tokenContext.Token = tokenDescriptor;
                    tokenContext.Replacement = tokenValue = tokenDescriptor.Value(context);
                    // the value of the token is in the following token type space
                    tokenType = tokenDescriptor.ValueType;
                }
            }
            if (!string.IsNullOrWhiteSpace(format)) {
                tokenContext.Replacement = string.Format("{0:" + format + "}", tokenContext.Replacement);
            }
            return tokenContext;
        }

        private static bool GetProperty(object obj, string name, out object value) {
            value = null;
            var pi = obj.GetType().GetProperty(name);
            if (pi == null) {
                return false;
            }
            value = pi.GetValue(obj, new object[] { });
            return true;
        }

        public string Replace(IEnumerable<TokenContext> tokens, string str) {
            var sb = new StringBuilder(str.Length);
            var offset = 0;
            foreach (var token in tokens) {
                // after last token and before current token
                sb.Append(str.Substring(offset, token.Offset - offset));
                offset = token.Offset + token.Length;
                // token value
                if (token.Replacement != null) {
                    sb.Append(token.Replacement);
                }
            }
            sb.Append(str.Substring(offset));
            return sb.ToString();
        }

        public string Replace(string str, object contexts) {
            return Replace(ParseTokens(str, contexts), str);
        }
        public string Replace(string str, IDictionary<string, string> contexts) {
            return Replace(ParseTokens(str, contexts), str);
        }
    }
}
