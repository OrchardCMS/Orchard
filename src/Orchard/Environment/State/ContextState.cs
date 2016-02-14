using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;

namespace Orchard.Environment.State {

    /// <summary>
    /// Holds some state through the logical call context
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

        public void SetState(T state) {
            CallContext.LogicalSetData(_name, new ObjectHandle(state));
        }
    }
}
