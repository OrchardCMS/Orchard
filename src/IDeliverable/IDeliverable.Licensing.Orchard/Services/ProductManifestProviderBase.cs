using System.Collections.Generic;
using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public abstract class ProductManifestProviderBase : IProductManifestProvider
    {
        public abstract IEnumerable<ProductManifest> GetProductManifests();
    }
}