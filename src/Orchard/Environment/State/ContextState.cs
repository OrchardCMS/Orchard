using System;
using System.Runtime.Remoting;
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
                // Because CallContext Logical Data can be shared across application domains,
                // here we also check if it's a real local instance, not a tranparent proxy.
                var handle = CallContext.LogicalGetData(_name) as ObjectHandle;
                var data = handle != null && !RemotingServices.IsTransparentProxy(handle) ? handle.Unwrap() : null;

                if (data == null) {
                    if (_defaultValue != null) {
                        // Because CallContext Logical Data can be shared across application domains,
                        // data are wrapped in an ObjectHandle that inherits from MarshalByRefObject.
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
    }
}
