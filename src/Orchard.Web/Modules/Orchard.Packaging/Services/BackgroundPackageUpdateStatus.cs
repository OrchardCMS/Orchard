using Orchard.Environment.Extensions;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    public interface IBackgroundPackageUpdateStatus : ISingletonDependency {
        PackagesStatusResult Value { get; set; }
    }

    [OrchardFeature("Gallery.Updates")]
    public class BackgroundPackageUpdateStatus : IBackgroundPackageUpdateStatus {
        public PackagesStatusResult Value { get; set; }
    }
}