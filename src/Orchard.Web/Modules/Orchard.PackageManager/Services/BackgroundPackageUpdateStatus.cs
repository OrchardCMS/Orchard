namespace Orchard.PackageManager.Services {
    public interface IBackgroundPackageUpdateStatus : ISingletonDependency {
        PackagesStatusResult Value { get; set; }
    }

    public class BackgroundPackageUpdateStatus : IBackgroundPackageUpdateStatus {
        public PackagesStatusResult Value { get; set; }
    }
}