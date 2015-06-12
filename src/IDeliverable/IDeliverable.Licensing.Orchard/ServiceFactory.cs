using System;
using System.Collections.Generic;
using IDeliverable.Licensing.Orchard.Services;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.VirtualPath;
using Orchard.Services;

namespace IDeliverable.Licensing.Orchard
{
    public class ServiceFactory
    {
        public static readonly ServiceFactory Current = new ServiceFactory();
        public static readonly IList<IProductManifestProvider> ProductManifestProviders = new List<IProductManifestProvider>();
        private readonly IDictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void RegisterProductManifestProvider<T>(T provider) where T : IProductManifestProvider
        {
            ProductManifestProviders.Add(provider);
        }

        public ServiceFactory()
        {
            var httpContextAccessor = RegisterService<IHttpContextAccessor, HttpContextAccessor>(new HttpContextAccessor());
            var cacheService = RegisterService<ICacheService, CacheService>(new CacheService(httpContextAccessor));
            var appDataFolderRoot = RegisterService<IAppDataFolderRoot, AppDataFolderRoot>(new AppDataFolderRoot());
            var clock = RegisterService<IClock, Clock>(new Clock());
            var virtualPathMonitor = RegisterService<IVirtualPathMonitor, DefaultVirtualPathMonitor>(new DefaultVirtualPathMonitor(clock));
            var appDataFolder = RegisterService<IAppDataFolder, AppDataFolder>(new AppDataFolder(appDataFolderRoot, virtualPathMonitor));
            var licenseFileManager = RegisterService<ILicenseFileManager, LicenseFileManager>(new LicenseFileManager(appDataFolder));
            var productManifestManager = RegisterService<IProductManifestManager, ProductManifestManager>(new ProductManifestManager(ProductManifestProviders));
            var licensingServiceClient = RegisterService<ILicensingServiceClient, LicensingServiceClient>(new LicensingServiceClient(httpContextAccessor));
            var licenseAccessor = RegisterService<ILicenseAccessor, LicenseAccessor>(new LicenseAccessor(licenseFileManager, productManifestManager, licensingServiceClient, cacheService));
            var licenseValidator = RegisterService<ILicenseValidator, LicenseValidator>(new LicenseValidator(httpContextAccessor, licenseAccessor, licenseFileManager));
        }

        public T Resolve<T>()
        {
            var service = _services.ContainsKey(typeof(T)) ? _services[typeof(T)] : default(T);
            return (T) service;
        }

        private TService RegisterService<TService, TImplementation>(TImplementation implementation) where TImplementation : TService
        {
            _services[typeof(TService)] = implementation;
            return implementation;
        }
    }
}