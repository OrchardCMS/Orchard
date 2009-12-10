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
                .Register(ctx => HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current))
                .As<HttpContextBase>()
                .FactoryScoped();
        }
    }
}