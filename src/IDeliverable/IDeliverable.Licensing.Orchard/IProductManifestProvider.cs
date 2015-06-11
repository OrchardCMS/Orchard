using System.Collections.Generic;

namespace IDeliverable.Licensing.Orchard
{
    public interface IProductManifestProvider
    {
        IEnumerable<ProductManifest> GetProductManifests();
    }
}