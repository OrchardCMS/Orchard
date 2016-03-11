using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orchard.Logging;
using Orchard.Exceptions;

namespace Orchard.Caching {
    public class DefaultAsyncTokenProvider : IAsyncTokenProvider {
        public DefaultAsyncTokenProvider() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IVolatileToken GetToken(Action<Action<IVolatileToken>> task) {
            var token = new AsyncVolativeToken(task, Logger);
            token.QueueWorkItem();
            return token;
        }

        public class AsyncVolativeToken : IVolatileToken {
            private readonly Action<Action<IVolatileToken>> _task;
            private readonly List<IVolatileToken> _taskTokens = new List<IVolatileToken>();
            private volatile Exception _taskException;
            private volatile bool _isTaskFinished;

            public AsyncVolativeToken(Action<Action<IVolatileToken>> task, ILogger logger) {
                _task = task;
                Logger = logger;
            }

            public ILogger Logger { get; set; }

            public void QueueWorkItem() {
                // Start a work item to collect tokens in our internal array
                ThreadPool.QueueUserWorkItem(state => {
                    try {
                        _task(token => _taskTokens.Add(token));
                    }
                    catch (Exception ex) {
                        if (ex.IsFatal()) {                 
                            throw;
                        }
                        Logger.Error(ex, "Error while monitoring extension files. Assuming extensions are not current.");
                        _taskException = ex;
                    }
                    finally {
                        _isTaskFinished = true;
                    }
                });
            }

            public bool IsCurrent {
                get {
                    // We are current until the task has finished
                    if (_taskException != null) {
                        return false;
                    }
                    if (_isTaskFinished) {
                        return _taskTokens.All(t => t.IsCurrent);
                    }
                    return true;
                }
            }
        }
    }
}