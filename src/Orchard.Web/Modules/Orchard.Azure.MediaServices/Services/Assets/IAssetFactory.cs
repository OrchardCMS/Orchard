using Orchard.Azure.MediaServices.Models.Assets;
using Orchard;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public interface IAssetFactory : IDependency {
        Asset Create(string type);
    }
}
