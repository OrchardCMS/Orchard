using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Tokens.Implementation {
    public class TokenManager : ITokenManager {
        private readonly IEnumerable<ITokenProvider> _providers;

        public TokenManager(IEnumerable<ITokenProvider> providers) {
            _providers = providers;
        }

        public IEnumerable<TokenTypeDescriptor> Describe(IEnumerable<string> targets) {
            var context = new DescribeContextImpl();
            foreach (var provider in _providers) {
                provider.Describe(context);
            }
            return context.Describe((targets ?? Enumerable.Empty<string>()).ToArray());
        }

        public IDictionary<string, object> Evaluate(string target, IDictionary<string, string> tokens, IDictionary<string, object> data) {
            var context = new EvaluateContextImpl(target, tokens, data, this);
            foreach (var provider in _providers) {
                provider.Evaluate(context);
            }
            return context.Produce();
        }

        private class EvaluateContextImpl : EvaluateContext {
            private readonly string _target;
            private readonly IDictionary<string, string> _tokens;
            private readonly IDictionary<string, object> _data;
            private readonly ITokenManager _manager;
            private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

            public EvaluateContextImpl(string target, IDictionary<string, string> tokens, IDictionary<string, object> data, ITokenManager manager) {
                _target = target;
                _tokens = tokens;
                _data = data;
                _manager = manager;
            }

            public IDictionary<string, object> Produce() {
                return _values;
            }

            public override string Target {
                get { return _target; }
            }

            public override IDictionary<string, string> Tokens {
                get { return _tokens; }
            }

            public override IDictionary<string, object> Data {
                get { return _data; }
            }

            public override IDictionary<string, object> Values {
                get { return _values; }
            }

            public override EvaluateFor<TData> For<TData>(string target) {
                if (_data != null && string.Equals(target, _target, StringComparison.InvariantCulture)) {
                    object value;
                    if (_data.TryGetValue(target, out value)) {
                        return new EvaluateForImpl<TData>(this, (TData)value);
                    }
                }

                return new EvaluateForSilent<TData>();
            }

            public override EvaluateFor<TData> For<TData>(string target, TData defaultData) {
                return For(target, () => defaultData);
            }

            public override EvaluateFor<TData> For<TData>(string target, Func<TData> defaultData) {
                if (string.Equals(target, _target, StringComparison.InvariantCulture)) {
                    var data = default(TData);
                    object value;
                    if (_data != null && _data.TryGetValue(target, out value)) {
                        data = (TData)value;
                    }
                    else if (defaultData != null) {
                        data = defaultData();
                    }

                    return new EvaluateForImpl<TData>(this, data);
                }

                return new EvaluateForSilent<TData>();
            }

            private class EvaluateForImpl<TData> : EvaluateFor<TData> {
                private readonly EvaluateContextImpl _context;
                private readonly TData _data;

                public EvaluateForImpl(EvaluateContextImpl context, TData data) {
                    _context = context;
                    _data = data;
                }

                public override TData Data {
                    get { return _data; }
                }

                public override EvaluateFor<TData> Token(string token, Func<TData, object> tokenValue) {
                    string originalToken;
                    if (_context.Tokens.TryGetValue(token, out originalToken)) {
                        try {
                            _context.Values[originalToken] = tokenValue(_data);
                        }
                        catch (NullReferenceException) {
                            _context.Values[originalToken] = null;
                        }
                    }
                    return this;
                }

                public override EvaluateFor<TData> Token(Func<string, TData, object> tokenValue) {
                    return Token(null, tokenValue);
                }

                public override EvaluateFor<TData> Token(Func<string, string> filter, Func<string, TData, object> tokenValue) {
                    foreach (var token in _context.Tokens) {
                        var tokenName = token.Key;
                        if (filter != null) {
                            tokenName = filter(token.Key);
                            if (tokenName == null)
                                continue;
                        }
                        var value = tokenValue(tokenName, _data);
                        if (value != null) {
                            _context.Values[token.Value] = value;
                        }
                    }
                    return this;
                }


                public override EvaluateFor<TData> Chain(string token, string chainTarget, Func<TData, object> chainValue) {
                    var subTokens = _context.Tokens
                        .Where(kv => kv.Key.StartsWith(token + "."))
                        .ToDictionary(kv => kv.Key.Substring(token.Length + 1), kv => kv.Value);
                    if (!subTokens.Any()) {
                        return this;
                    }
                    var subValues = _context._manager.Evaluate(chainTarget, subTokens, new Dictionary<string, object> { { chainTarget, chainValue(_data) } });
                    foreach (var subValue in subValues) {
                        _context.Values[subValue.Key] = subValue.Value;
                    }
                    return this;
                }

                public override EvaluateFor<TData> Chain(Func<string, Tuple<string, string>> filter, string chainTarget, Func<string, TData, object> chainValue) {
                    var subTokens = _context.Tokens
                        .Where(kv => kv.Key.Contains('.'))
                        .Select(kv => {
                            var filterResult = filter(kv.Key);
                            return filterResult != null ? new Tuple<string, string, string>(filterResult.Item1, filterResult.Item2, kv.Value) : null;
                        })
                        .Where(st => st != null)
                        .ToList();
                    if (!subTokens.Any()) {
                        return this;
                    }
                    foreach(var chainGroup in subTokens.GroupBy(st => st.Item1)) {
                        var subValues = _context._manager.Evaluate(chainTarget, chainGroup.ToDictionary(cg => cg.Item2, cg => cg.Item3), new Dictionary<string, object> { { chainTarget, chainValue(chainGroup.Key, _data) } });
                        foreach (var subValue in subValues) {
                            _context.Values[subValue.Key] = subValue.Value;
                        }
                    }
                    return this;
                }
            }

            private class EvaluateForSilent<TData> : EvaluateFor<TData> {
                public override TData Data {
                    get { return default(TData); }
                }

                public override EvaluateFor<TData> Token(string token, Func<TData, object> tokenValue) {
                    return this;
                }

                public override EvaluateFor<TData> Token(Func<string, TData, object> tokenValue) {
                    return this;
                }

                public override EvaluateFor<TData> Token(Func<string, string> filter, Func<string, TData, object> tokenValue) {
                    return this;
                }

                public override EvaluateFor<TData> Chain(string token, string chainTarget, Func<TData, object> chainValue) {
                    return this;
                }

                public override EvaluateFor<TData> Chain(Func<string, Tuple<string, string>> filter, string chainTarget, Func<string, TData, object> chainValue) {
                    return this;
                }
            }
        }

        private class DescribeContextImpl : DescribeContext {
            private readonly Dictionary<string, DescribeFor> _describes = new Dictionary<string, DescribeFor>();

            public override IEnumerable<TokenTypeDescriptor> Describe(params string[] targets) {
                return _describes
                    .Where(kp => targets == null || targets.Length == 0 || targets.Contains(kp.Key))
                    .Select(kp => new TokenTypeDescriptor {
                    Target = kp.Key,
                    Name = kp.Value.Name,
                    Description = kp.Value.Description,
                    Tokens = kp.Value.Tokens
                });
            }

            public override DescribeFor For(string target) {
                return For(target, null, null);
            }

            public override DescribeFor For(string target, LocalizedString name, LocalizedString description) {
                DescribeFor describeFor;
                if (!_describes.TryGetValue(target, out describeFor)) {
                    describeFor = new DescribeForImpl(target, name, description);
                    _describes[target] = describeFor;
                }
                return describeFor;
            }
        }

        private class DescribeForImpl : DescribeFor {
            private readonly LocalizedString _name;
            private readonly LocalizedString _description;
            private readonly string _target;
            private readonly List<TokenDescriptor> _tokens = new List<TokenDescriptor>();

            public DescribeForImpl(string target, LocalizedString name, LocalizedString description) {
                _target = target;
                _name = name;
                _description = description;
            }

            public override LocalizedString Name {
                get {
                    return _name;
                }
            }
            public override LocalizedString Description {
                get {
                    return _description;
                }
            }

            public override IEnumerable<TokenDescriptor> Tokens {
                get { return _tokens; }
            }

            public override DescribeFor Token(string token, LocalizedString name, LocalizedString description) {
                return Token(token, name, description, null);
            }

            public override DescribeFor Token(string token, LocalizedString name, LocalizedString description, string chainTarget) {
                _tokens.Add(new TokenDescriptor { Token = token, Name = name, Description = description, Target = _target, ChainTarget = chainTarget });
                return this;
            }

        }
    }
}