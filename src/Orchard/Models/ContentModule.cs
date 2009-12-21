using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;

namespace Orchard.ContentManagement {
    public class ContentModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.Register<DefaultContentQuery>().As<IContentQuery>().FactoryScoped();
        }
    }
}
