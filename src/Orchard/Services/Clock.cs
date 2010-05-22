using System;
using Orchard.Caching;

namespace Orchard.Services {
    public interface IClock : IVolatileProvider {
        DateTime UtcNow { get; }
    }

    public class Clock : IClock {
        public DateTime UtcNow {
            get { return DateTime.UtcNow; }
        }

    }
}
