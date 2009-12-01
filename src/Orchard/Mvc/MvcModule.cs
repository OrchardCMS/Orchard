using System.Linq;
using System.Web;
using Autofac.Builder;
using Autofac.Integration.Web.Mvc;
using Orchard.Controllers;
using Orchard.Environment;
using Orchard.Mvc.Filters;
using Orchard.Packages;

namespace Orchard.Mvc {
    public class MvcModule : Module {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IPackageManager _packageManager;

        public MvcModule(ICompositionStrategy compositionStrategy, IPackageManager packageManager) {
            _compositionStrategy = compositionStrategy;
            _packageManager = packageManager;
        }

        protected override void Load(ContainerBuilder moduleBuilder) {
            var packages = _packageManager.ActivePackages();
            var assemblies = packages.Select(x => x.Assembly).Concat(new[] { typeof(HomeController).Assembly });

            var module = new AutofacControllerModule(assemblies.ToArray()) {
                ActionInvokerType = typeof(FilterResolvingActionInvoker),
                IdentificationStrategy = new OrchardControllerIdentificationStrategy(packages)
            };

            moduleBuilder.RegisterModule(module);
            moduleBuilder
                .Register(ctx => HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current))
                .As<HttpContextBase>()
                .FactoryScoped();
        }
    }
}