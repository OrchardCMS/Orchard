using Autofac;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides
{
    public class ServiceContainer : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ServiceFactory.RegisterProductManifestProvider(new SlidesProductManifestProvider());
        }
    }
}