using System;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Orchard.Mvc.Extensions;

namespace Orchard.Environment.State {
    /// <summary>
    /// Holds some state for the current HttpContext or Logical Context
    /// </summary>
    /// <typeparam name="T">The type of data to store</typeparam>
    public class ContextState<T> where T : class {
        private readonly string _name;
        private readonly Func<T> _defaultValue;

        public ContextState(string name) {
            _name = name;
        }

        public ContextState(string name, Func<T> defaultValue) {
            _name = name;
            _defaultValue = defaultValue;
        }

        public T GetState() {
            if (HttpContext.Current.IsBackgroundHttpContext()) {
                var handle = CallContext.LogicalGetData(_name) as ObjectHandle;
                var data = handle != null ? handle.Unwrap() : null;

                if (data == null) {
                    if (_defaultValue != null) {
                        CallContext.LogicalSetData(_name, new ObjectHandle(data = _defaultValue()));
                        return data as T;
                    }
                }

                return data as T;
            }

            if (HttpContext.Current.Items[_name] == null) {
                HttpContext.Current.Items[_name] = _defaultValue == null ? null : _defaultValue();
            }

            return HttpContext.Current.Items[_name] as T;
        }

        public void SetState(T state) {
            if (HttpContext.Current.IsBackgroundHttpContext()) {
                CallContext.LogicalSetData(_name, new ObjectHandle(state));
            }
            else {
                HttpContext.Current.Items[_name] = state;
            }
        }

        internal class ObjectHandle : System.Runtime.Remoting.ObjectHandle {
            public ObjectHandle(object o) : base(o) { }
            public override object InitializeLifetimeService() {
                return null;
            }
        }
    }
}
