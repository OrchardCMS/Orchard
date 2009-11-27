using System;
using System.Collections.Generic;
using Orchard.Logging;

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
                    logger.Error(ex, "{2} thrown from {0} by {1}", 
                        typeof(TEvents).Name, 
                        sink.GetType().FullName, 
                        ex.GetType().Name);
                }
            }
        }
    }
}
