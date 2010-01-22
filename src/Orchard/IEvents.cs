using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Orchard.Logging;
using Orchard.Security;

namespace Orchard {
    public interface IEvents : IDependency {
    }

    public static class EventsExtensions {
        public static void Invoke<TEvents>(this IEnumerable<TEvents> events, Action<TEvents> dispatch, ILogger logger) where TEvents : IEvents {
            foreach (var sink in events) {
                try {
                    dispatch(sink);
                }
                catch (Exception ex) {
                    if (IsLogged(ex)) {
                        logger.Error(ex, "{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name);
                    }

                    if (IsFatal(ex)) {
                        throw;
                    }
                }
            }
        }

        private static bool IsLogged(Exception ex) {
            return ex is OrchardSecurityException || !IsFatal(ex);
        }

        private static bool IsFatal(Exception ex) {
            return ex is OrchardSecurityException ||
                ex is StackOverflowException ||
                ex is AccessViolationException ||
                ex is AppDomainUnloadedException ||
                ex is ExecutionEngineException ||
                ex is ThreadAbortException ||
                ex is SecurityException ||
                ex is SEHException;
        }
    }
}
