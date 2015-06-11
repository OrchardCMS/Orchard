using Autofac;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides
{
    public class ServiceContainer : Module
    {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterTypes(typeof (SlidesProductManifest)).As<IProductManifestProvider>();
            builder.RegisterType<ProductManifestManager>().As<IProductManifestManager>();
            builder.RegisterType<LicenseFileService>().As<ILicenseFileService>();
            builder.RegisterType<FileBasedLicenseAccessor>().As<ILicenseAccessor>();
            builder.RegisterType<LicenseValidator>().As<ILicenseValidator>();
        }
    }
}