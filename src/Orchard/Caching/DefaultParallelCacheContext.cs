using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Caching {
    public class DefaultParallelCacheContext : IParallelCacheContext {
        private readonly ICacheContextAccessor _cacheContextAccessor;

        public DefaultParallelCacheContext(ICacheContextAccessor cacheContextAccessor) {
            _cacheContextAccessor = cacheContextAccessor;
        }

        /// <summary>
        ///  Allow disabling parallel behavior through HostComponents.config
        /// </summary>
        public bool Disabled { get; set; }

        public IEnumerable<TResult> RunInParallel<T, TResult>(IEnumerable<T> source, Func<T, TResult> selector) {
            if (Disabled) {
                return source.Select(selector);
            }
            else {
                // Create tasks that capture the current thread context
                var tasks = source.Select(item => this.CreateContextAwareTask(() => selector(item))).ToList();

                // Run tasks in parallel and combine results immediately
                var result = tasks
                    .AsParallel() // prepare for parallel execution
                    .AsOrdered() // preserve initial enumeration order
                    .Select(task => task.Execute()) // prepare tasks to run in parallel
                    .ToArray(); // force evaluation

                // Forward tokens collected by tasks to the current context
                foreach (var task in tasks) {
                    task.Finish();
                }
                return result;
            }
        }

        /// <summary>
        /// Create a task that wraps some piece of code that implictly depends on the cache context.
        /// The return task can be used in any execution thread (e.g. System.Threading.Tasks).
        /// </summary>
        public ITask<T> CreateContextAwareTask<T>(Func<T> function) {
            return new TaskWithAcquireContext<T>(_cacheContextAccessor, function);
        }

        public class TaskWithAcquireContext<T> : ITask<T> {
            private readonly ICacheContextAccessor _cacheContextAccessor;
            private readonly Func<T> _function;
            private IList<IVolatileToken> _tokens;

            public TaskWithAcquireContext(ICacheContextAccessor cacheContextAccessor, Func<T> function) {
                _cacheContextAccessor = cacheContextAccessor;
                _function = function;
            }

            /// <summary>
            /// Execute task and collect eventual volatile tokens
            /// </summary>
            public T Execute() {
                IAcquireContext parentContext = _cacheContextAccessor.Current;
                try {
                    // Push context
                    if (parentContext == null) {
                        _cacheContextAccessor.Current = new SimpleAcquireContext(AddToken);
                    }

                    // Execute lambda
                    return _function();
                }
                finally {
                    // Pop context
                    if (parentContext == null) {
                        _cacheContextAccessor.Current = parentContext;
                    }
                }
            }

            /// <summary>
            /// Return tokens collected during task execution
            /// </summary>
            public IEnumerable<IVolatileToken> Tokens {
                get {
                    if (_tokens == null)
                        return Enumerable.Empty<IVolatileToken>();
                    return _tokens;
                }
            }

            public void Dispose() {
                Finish();
            }

            /// <summary>
            /// Forward collected tokens to current cache context
            /// </summary>
            public void Finish() {
                var tokens = _tokens;
                _tokens = null;
                if (_cacheContextAccessor.Current != null && tokens != null) {
                    foreach (var token in tokens) {
                        _cacheContextAccessor.Current.Monitor(token);
                    }
                }
            }

            private void AddToken(IVolatileToken token) {
                if (_tokens == null)
                    _tokens = new List<IVolatileToken>();
                _tokens.Add(token);
            }
        }
    }
}