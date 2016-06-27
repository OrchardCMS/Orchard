using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;

namespace Orchard.Tests.Stubs {
    public class StubCacheManager : ICacheManager {
        private readonly ICacheManager _defaultCacheManager;

        public StubCacheManager() {
            _defaultCacheManager = new DefaultCacheManager(this.GetType(), new DefaultCacheHolder(new DefaultCacheContextAccessor()));
        }
        public TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            return _defaultCacheManager.Get(key, acquire);
        }

        public ICache<TKey, TResult> GetCache<TKey, TResult>() {
            return _defaultCacheManager.GetCache<TKey, TResult>();
        }
    }

    public class StubParallelCacheContext : IParallelCacheContext {
        public ITask<T> CreateContextAwareTask<T>(Func<T> function) {
            throw new NotImplementedException();
        }

        public IEnumerable<TResult> RunInParallel<T, TResult>(IEnumerable<T> source, Func<T, TResult> selector) {
            return source.Select(selector);
        }
    }


    public class StubAsyncTokenProvider : IAsyncTokenProvider {
        public IVolatileToken GetToken(Action<Action<IVolatileToken>> task) {
            var tokens = new List<IVolatileToken>();
            task(tokens.Add);
            return new Token(tokens);
        }
        public class Token : IVolatileToken {
            private readonly List<IVolatileToken> _tokens;

            public Token(List<IVolatileToken> tokens) {
                _tokens = tokens;
            }

            public bool IsCurrent { get { return _tokens.All(t => t.IsCurrent); } }
        }
    }
}
