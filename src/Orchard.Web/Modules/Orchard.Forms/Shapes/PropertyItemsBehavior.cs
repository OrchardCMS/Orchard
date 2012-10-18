using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement;
using ClaySharp;

namespace Orchard.Forms.Shapes {
    public class PropertiesAreItems : ClayBehavior {
        public override object SetMember(Func<object> proceed, dynamic self, string name, object value) {
            Patch(self, name, value);
            return proceed();
        }

        public override object SetIndex(Func<object> proceed, dynamic self, IEnumerable<object> keys, object value) {
            if (keys.Count() == 1 && keys.All(k => k is string))
                Patch(self, System.Convert.ToString(keys.Single()), value);
            return proceed();
        }

        public override object InvokeMember(Func<object> proceed, dynamic self, string name, INamedEnumerable<object> args) {
            if (args.Count() == 1 && args.Named.Count() == 0)
                Patch(self, name, args.Single());
            return proceed();
        }

        readonly IDictionary<string, object> _assigned = new Dictionary<string, object>();
        private void Patch(dynamic self, string name, object value) {
            if (!name.StartsWith("_"))
                return;

            object priorValue;
            if (_assigned.TryGetValue(name, out priorValue) && priorValue != null) {
                // it's a no-op to reassign same value to a prop
                if (priorValue == value)
                    return;

                self.Items.Remove(priorValue);
            }
            if (value is IShape) {
                self.Items.Add(value);
            }
            _assigned[name] = value;
        }
    }
}