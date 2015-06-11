using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Azure.MediaServices.Infrastructure.Assets {
    public abstract class AssetDriver<TAsset> : IAssetDriver where TAsset : Asset {
        
        protected AssetDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public virtual string Prefix {
            get { return GetType().Name; }
        }

        IEnumerable<AssetDriverResult> IAssetDriver.BuildEditor(Asset asset, dynamic shapeFactory) {
            return BuildEditorInternal(asset, a => Editor(a, shapeFactory));
        }

        IEnumerable<AssetDriverResult> IAssetDriver.UpdateEditor(Asset asset, IUpdateModel updater, dynamic shapeFactory) {
            return BuildEditorInternal(asset, a => Editor(a, updater, shapeFactory));
        }

        protected virtual IEnumerable<AssetDriverResult> Editor(TAsset asset, dynamic shapeFactory) {
            return Enumerable.Empty<AssetDriverResult>();
        }

        protected virtual IEnumerable<AssetDriverResult> Editor(TAsset asset, IUpdateModel updater, dynamic shapeFactory) {
            return Enumerable.Empty<AssetDriverResult>();
        }

        private static IEnumerable<AssetDriverResult> BuildEditorInternal(Asset asset, Func<TAsset, IEnumerable<AssetDriverResult>> harvestResults) {
            var a = asset as TAsset;

            if (a == null)
                return Enumerable.Empty<AssetDriverResult>();

            var results = harvestResults(a).ToArray();

            foreach (var result in results) {
                if (result.EditorShape != null)
                    result.EditorShape.Asset = a;
            }

            return results;
        }
    }
}