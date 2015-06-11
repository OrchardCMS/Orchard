using System.Collections.Generic;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class ProductManifestProviderBase : IProductManifestProvider
    {
        public abstract IEnumerable<ProductManifest> GetProductManifests();
    }
}