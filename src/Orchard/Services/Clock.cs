using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Services {
    public interface IClock : IDependency {
        DateTime UtcNow { get; }
    }

    public class Clock : IClock {
        public DateTime UtcNow {
            get { return DateTime.Now; }
        }
    }
}
