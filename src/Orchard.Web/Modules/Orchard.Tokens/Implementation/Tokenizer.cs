using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Orchard.Tokens.Implementation {
    public class Tokenizer : ITokenizer {
        private readonly ITokenManager _tokenManager;

        public Tokenizer(ITokenManager tokenManager) {
            _tokenManager = tokenManager;
        }

        public IDictionary<string, object> Evaluate(IEnumerable<string> tokens, object data) {
            return Evaluate(tokens, new RouteValueDictionary(data));
        }

        public IDictionary<string, object> Evaluate(IEnumerable<string> tokens, IDictionary<string, object> data) {
            var distinctTokens = tokens.Distinct().ToList();
            var replacements = distinctTokens.ToDictionary(s => s, s => (object)null);
            return distinctTokens
                .Select(Split)
                .GroupBy(item => item.Item1)
                .SelectMany(grouping => _tokenManager.Evaluate(grouping.Key, grouping.ToDictionary(item => item.Item2, item => item.Item3), data))
                .Aggregate(replacements, (agg, kv) => {
                    agg[kv.Key] = kv.Value;
                    return agg;
                });
        }

        public string Replace(string text, object data) {
            return Replace(text, data, ReplaceOptions.Default);
        }

        public string Replace(string text, object data, ReplaceOptions options) {
            return Replace(text, new RouteValueDictionary(data), options);
        }

        public string Replace(string text, IDictionary<string, object> data) {
            return Replace(text, data, ReplaceOptions.Default);
        }

        public string Replace(string text, IDictionary<string, object> data, ReplaceOptions options) {
            var tokenset = Parse(text);
            var tokens = tokenset.Item2;
            var replacements = Evaluate(options.Predicate == null ? tokens : tokens.Where(options.Predicate), data);

            return replacements.Aggregate(tokenset.Item1,
                (current, replacement) => current.Replace("{" + replacement.Key + "}", (options.Encoding ?? ReplaceOptions.NoEncode)(replacement.Key, replacement.Value)));
        }

        private static Tuple<string, IEnumerable<string>> Parse(string text) {
            var tokens = new List<string>();
            if (!string.IsNullOrEmpty(text)) {
                var inToken = false;
                var tokenStart = 0;
                for (var i = 0; i < text.Length; i++) {
                    var c = text[i];
                    if (c == '{') {
                        if (i + 1 < text.Length && text[i + 1] == '{') {
                            text = text.Substring(0, i) + text.Substring(i + 1);
                            continue;
                        }
                    }
                    else if (c == '}') {
                        if (i + 1 < text.Length && text[i + 1] == '}') {
                            text = text.Substring(0, i) + text.Substring(i + 1);
                            continue;
                        }
                    }

                    if (inToken) {
                        if (c == '}') {
                            inToken = false;
                            var token = text.Substring(tokenStart + 1, i - tokenStart - 1);
                            tokens.Add(token);
                        }
                    }
                    else if (c == '{') {
                        inToken = true;
                        tokenStart = i;
                    }
                }
            }
            return new Tuple<string, IEnumerable<string>>(text, tokens);
        }

        private static Tuple<string, string, string> Split(string token) {
            var dotIndex = token.IndexOf('.');
            if (dotIndex != -1) {
                return Tuple.Create(token.Substring(0, dotIndex), token.Substring(dotIndex + 1), token);
            }
            return Tuple.Create(token, "", token);
        }

    }
}