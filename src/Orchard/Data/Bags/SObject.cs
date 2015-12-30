using System.Collections.Generic;
using System.Dynamic;

namespace Orchard.Data.Bags {
    public class Bag : DynamicObject, IEnumerable<KeyValuePair<string, object>>, ISItem {
        internal readonly Dictionary<string, ISItem> _properties = new Dictionary<string, ISItem>();

        public static dynamic New() {
            return new Bag();
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            return SetMember(binder.Name, value);
        }

        public bool SetMember(string name, object value) {
            if (value == null && _properties.ContainsKey(name)) {
                _properties.Remove(name);
            }
            else {
                _properties[name] = SConvert.ToSettings(value);
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            return GetMember(binder.Name, out result);
        }

        public bool GetMember(string name, out object result) {
            // if the property doesn't exist, return null
            if (!_properties.ContainsKey(name)) {
                result = null;
                return true;
            }

            var value = _properties[name];

            result = SConvert.ToObject(value);

            return true;
        }

        // Set the property value by index.
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            return SetMember(indexes[0].ToString(), value);
        }

        // Get the property value by index.
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            return GetMember(indexes[0].ToString(), out result);
        }

        public static Bag operator &(Bag o1, Bag o2) {
            if (o1 == null) {
                return o2;
            }

            var clone = (Bag)o1.Clone();
            dynamic dclone = clone;

            foreach (var pair in o2._properties) {
                if (clone._properties.ContainsKey(pair.Key)) {
                    if (pair.Value == null) {
                        // remove the left element
                        dclone[pair.Key] = null;
                    }
                    else {
                        dynamic d1 = clone._properties[pair.Key];
                        dclone[pair.Key] = d1 & (dynamic)pair.Value;
                    }
                }
                else {
                    if (pair.Value != null) {
                        dclone[pair.Key] = pair.Value;
                    }
                }
            }

            return clone;
        }

        #region IEnumerable<KeyValuePair<string,object>> Members

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            foreach (var key in _properties.Keys) {
                object member;
                if (GetMember(key, out member)) {
                    yield return new KeyValuePair<string, object>(key, member);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region ICloneable Members

        public object Clone() {
            var o = New();
            foreach (var pair in _properties) {
                o[pair.Key] = pair.Value.Clone();
            }

            return o;
        }

        #endregion
    }
}