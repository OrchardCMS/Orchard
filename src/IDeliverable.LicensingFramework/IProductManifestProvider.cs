using System.Collections.Generic;
using Orchard;

namespace IDeliverable.Licensing
{
    public interface IProductManifestProvider : IDependency
    {
        IEnumerable<ProductManifest> GetProductManifests();
    }
}