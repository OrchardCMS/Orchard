using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Forms.Shapes {
    public class PropertiesAreItems : Shape {
        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value) {
            Patch(this, binder.Name, value);
            return base.TrySetMember(binder, value);
        }

        public override bool TrySetIndex(System.Dynamic.SetIndexBinder binder, object[] indexes, object value) {
            if (indexes.Count() == 1 && indexes.All(k => k is string))
                Patch(this, System.Convert.ToString(indexes.Single()), value);
            return base.TrySetIndex(binder, indexes, value);
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result) {
            var arguments = Arguments.From(args, binder.CallInfo.ArgumentNames);
            if (args.Count() == 1 && !arguments.Named.Any())
                Patch(this, binder.Name, args.Single());
            return base.TryInvokeMember(binder, args, out result);
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