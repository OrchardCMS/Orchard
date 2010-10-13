using System;
using System.Web;

namespace Orchard.Tests.Stubs {
    public class StubWorkContextAccessor : IWorkContextAccessor {
        public WorkContext GetContext(HttpContextBase httpContext) {
            throw new NotSupportedException();
        }

        public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
            throw new NotSupportedException();
        }

        public WorkContext GetContext() {
            return null;
        }

        public IWorkContextScope CreateWorkContextScope() {
            throw new NotSupportedException();
        }
    }
}
