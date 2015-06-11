using Orchard.Azure.MediaServices.Models.Assets;
using Orchard;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public interface IAssetStorageProvider : IDependency {
        void BindStorage(Asset asset);
    }
}
