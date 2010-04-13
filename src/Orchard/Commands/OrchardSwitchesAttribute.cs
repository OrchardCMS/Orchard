using System;
using System.Collections.Generic;

namespace Orchard.Commands {
    [AttributeUsage(AttributeTargets.Method)]
    public class OrchardSwitchesAttribute : Attribute {
        private readonly IEnumerable<string> _switches;

        public OrchardSwitchesAttribute(string switches) {
            List<string> switchList = new List<string>();
            foreach (var s in switches.Split(',')) {
                switchList.Add(s.Trim());
            }
            _switches = switchList;
        }

        public IEnumerable<string> SwitchName {
            get { return _switches; }
        }

    }
}
