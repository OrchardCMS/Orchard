using System;
using Orchard.Azure.MediaServices.Models.Assets;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public class AssetFactory : IAssetFactory {
        public Asset Create(string typeName) {
            var type = Type.GetType(typeName);
            return (Asset)Activator.CreateInstance(type);
        }
    }
}