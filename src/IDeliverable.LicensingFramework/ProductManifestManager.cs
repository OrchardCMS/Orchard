using System;
using System.Collections.Generic;
using System.Linq;

namespace IDeliverable.Licensing
{
    public class ProductManifestManager : IProductManifestManager
    {
        private readonly IEnumerable<IProductManifestProvider> _providers;

        public ProductManifestManager(IEnumerable<IProductManifestProvider> providers)
        {
            _providers = providers;
        }

        public IEnumerable<ProductManifest> GetProductManifests()
        {
            return _providers.SelectMany(x => x.GetProductManifests());
        }

        public ProductManifest FindByProductId(int productId)
        {
            return Find(x =>  x.ProductId == productId);
        }

        public ProductManifest FindByExtensionName(string extensionName)
        {
            return Find(x => x.ExtensionName == extensionName);
        }

        public ProductManifest Find(Func<ProductManifest, bool> predicate)
        {
            return GetProductManifests().SingleOrDefault(predicate);
        }
    }
}