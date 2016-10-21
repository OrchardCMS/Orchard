using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Orchard.OutputCache {
    public interface ICachingEventHandler : IEventHandler {
        void KeyGenerated(StringBuilder key);
    }
}