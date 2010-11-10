using System;
using Orchard.Caching;

namespace Orchard.Services {
    public interface IClock : IVolatileProvider {
        DateTime UtcNow { get; }

        /// <summary>
        /// Each retrieved value is cached during the specified amount of time.
        /// </summary>
        IVolatileToken When(TimeSpan duration);

        /// <summary>
        /// The cache is active until the specified time. Each subsequent access won't be cached.
        /// </summary>
        IVolatileToken WhenUtc(DateTime absoluteUtc);
    }

    public class Clock : IClock {
        public DateTime UtcNow {
            get { return DateTime.UtcNow; }
        }

        public IVolatileToken When(TimeSpan duration) {
            return new AbsoluteExpirationToken(this, duration);
        }

        public IVolatileToken WhenUtc(DateTime absoluteUtc) {
            return new AbsoluteExpirationToken(this, absoluteUtc);
        }

        public class AbsoluteExpirationToken : IVolatileToken {
            private readonly IClock _clock;
            private readonly DateTime _invalidateUtc;

            public AbsoluteExpirationToken(IClock clock, DateTime invalidateUtc) {
                _clock = clock;
                _invalidateUtc = invalidateUtc;
            }

            public AbsoluteExpirationToken(IClock clock, TimeSpan duration) {
                _clock = clock;
                _invalidateUtc = _clock.UtcNow.Add(duration);
            }

            public bool IsCurrent {
                get {
                    return _clock.UtcNow < _invalidateUtc;
                }
            }
        }
    }
}
