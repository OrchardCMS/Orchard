using System.Collections.Generic;
using Orchard;

namespace IDeliverable.Licensing.Orchard
{
    public interface IProductManifestProvider : IDependency
    {
        IEnumerable<ProductManifest> GetProductManifests();
    }
}