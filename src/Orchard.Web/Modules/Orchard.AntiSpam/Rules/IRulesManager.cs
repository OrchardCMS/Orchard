using System;
using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.AntiSpam.Rules {
    public interface IRulesManager : IEventHandler {
        void TriggerEvent(string category, string type, Func<Dictionary<string, object>> tokensContext);
    }

}