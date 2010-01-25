using System.Linq;
using System.Web;
using Autofac.Builder;
using Autofac.Integration.Web.Mvc;
using Orchard.Controllers;
using Orchard.Environment;
using Orchard.Mvc.Filters;
using Orchard.Extensions;

namespace Orchard.Mvc {
    public class MvcModule : Module {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IExtensionManager _extensionManager;

        public MvcModule(ICompositionStrategy compositionStrategy, IExtensionManager extensionManager) {
            _compositionStrategy = compositionStrategy;
            _extensionManager = extensionManager;
        }

        protected override void Load(ContainerBuilder moduleBuilder) {
            var extensions = _extensionManager.ActiveExtensions();
            var assemblies = extensions.Select(x => x.Assembly).Concat(new[] { typeof(HomeController).Assembly });

            var module = new AutofacControllerModule(assemblies.ToArray()) {
                ActionInvokerType = typeof(FilterResolvingActionInvoker),
                IdentificationStrategy = new OrchardControllerIdentificationStrategy(extensions)
            };

            moduleBuilder.RegisterModule(module);
            moduleBuilder
                .Register(ctx => HttpContext.Current == null ? (HttpContextBase)new HttpContextPlaceholder() : new HttpContextWrapper(HttpContext.Current))
                .As<HttpContextBase>()
                .FactoryScoped();
        }

        /// <summary>
        /// standin context for background tasks.
        /// </summary>
        class HttpContextPlaceholder : HttpContextBase {
            public override HttpRequestBase Request {
                get { return new HttpRequestPlaceholder(); }
            }
        }

        /// <summary>
        /// standin context for background tasks. 
        /// </summary>
        class HttpRequestPlaceholder : HttpRequestBase {
            /// <summary>
            /// anonymous identity provided for background task.
            /// </summary>
            public override bool IsAuthenticated {
                get { return false; }
            }
        }
    }


}