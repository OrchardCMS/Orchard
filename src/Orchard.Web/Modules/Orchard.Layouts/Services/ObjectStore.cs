using System;
using System.Web;
using Orchard.Validation;

namespace Orchard.Layouts.Services {
    public class ObjectStore : IObjectStore {
        private readonly IWorkContextAccessor _wca;

        public ObjectStore(IWorkContextAccessor wca) {
            _wca = wca;
        }

        private HttpSessionStateBase Session {
            get { return _wca.GetContext().HttpContext.Session; }
        }

        public void Set(string key, object value) {
            Argument.ThrowIfNull(key, "key");
            Session[key] = value;
        }

        public object Get(string key, Func<object> defaultValue = null) {
            Argument.ThrowIfNull(key, "key");
            return Session[key] ?? (defaultValue != null ? defaultValue() : null);
        }

        public string GenerateKey() {
            return Guid.NewGuid().ToString();
        }

        public void Remove(string key) {
            Session.Remove(key);
        }
    }
}