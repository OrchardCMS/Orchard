using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Integration.Web.Mvc;
using Orchard.Environment;
using Orchard.Mvc.Filters;

namespace Orchard.Mvc {
    public class MvcModule : Module {
        private readonly ICompositionStrategy _compositionStrategy;

        public MvcModule(ICompositionStrategy compositionStrategy) {
            _compositionStrategy = compositionStrategy;
        }

        protected override void Load(ContainerBuilder moduleBuilder) {
            var assemblies = _compositionStrategy.GetAssemblies().ToArray();

            var module = new AutofacControllerModule(assemblies) {
                ActionInvokerType = typeof(FilterResolvingActionInvoker),
                IdentificationStrategy = new OrchardControllerIdentificationStrategy() 
            };

            moduleBuilder.RegisterModule(module);
        }
    }
}