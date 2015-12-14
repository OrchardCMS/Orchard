using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Orchard.Data.Bags {
    public class SArray : DynamicObject, ISItem {
        public ISItem[] Values { get; private set; }

        public SArray(Array array) {
            Values = new ISItem[array.Length];
            for (var i = 0; i < array.Length; i++) {
                Values[i] = SConvert.ToSettings(array.GetValue(i));
            }
        }

        // Set the property value by index.
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            if (typeof(long).IsAssignableFrom(indexes[0].GetType())) {
                Values[(long)indexes[0]] = SConvert.ToSettings(value);
                return true;
            }

            return false;
        }

        // Get the property value by index.
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            if (typeof(long).IsAssignableFrom(indexes[0].GetType())) {
                result = SConvert.ToObject(Values[(long)indexes[0]]);
                return true;
            }

            result = null;
            return false;
        }

        // Forward any call to the internal array (Length, ...)
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            try {
                result = typeof(Array).InvokeMember(
                          binder.Name,
                          BindingFlags.InvokeMethod |
                          BindingFlags.Public |
                          BindingFlags.Instance,
                          null, Values, args);
                return true;
            }
            catch {
                result = null;
                return false;
            }
        }

        #region ICloneable Members

        public object Clone() {
            return Values.Select(x => x.Clone()).Cast<ISItem>().ToArray();
        }

        #endregion

        public static SArray operator &(SArray o1, SArray o2) {
            // concatenate the value with the array
            return new SArray(o1.Values.Union(o2.Values).ToArray());
        }

        public static SArray operator &(SArray o1, SValue o2) {
            // concatenate the value with the array
            return new SArray(o1.Values.Union(new[] { o1 }).ToArray());
        }

        public static Bag operator &(SArray o1, Bag o2) {
            return o2;
        }

    }
}