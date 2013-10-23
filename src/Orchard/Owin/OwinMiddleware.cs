using System;
using Owin;

namespace Orchard.Owin {
    public class OwinMiddleware {
        public Action<IAppBuilder> Configure { get; set; }
        public string Priority { get; set; }
    }
}
