using System;
using System.Web;

namespace Orchard {
    public class StaticHttpContextScope : IDisposable {
        private readonly HttpContext _previousHttpContext;

        public StaticHttpContextScope(HttpContext stub) {
            _previousHttpContext = HttpContext.Current;
            HttpContext.Current = stub;
        }

        public void Dispose() {
            HttpContext.Current = _previousHttpContext;
        }
    }
}