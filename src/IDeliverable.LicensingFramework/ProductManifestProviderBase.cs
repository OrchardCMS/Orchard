using System.Collections.Generic;

namespace IDeliverable.Licensing
{
    public abstract class ProductManifestProviderBase : IProductManifestProvider
    {
        public abstract IEnumerable<ProductManifest> GetProductManifests();
    }
}