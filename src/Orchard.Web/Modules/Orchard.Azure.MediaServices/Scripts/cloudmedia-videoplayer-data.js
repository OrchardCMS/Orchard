var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (VideoPlayer) {
                (function (Data) {
                    (function (AssetType) {
                        AssetType[AssetType["VideoAsset"] = 0] = "VideoAsset";
                        AssetType[AssetType["DynamicVideoAsset"] = 1] = "DynamicVideoAsset";
                        AssetType[AssetType["ThumbnailAsset"] = 2] = "ThumbnailAsset";
                        AssetType[AssetType["SubtitleAsset"] = 3] = "SubtitleAsset";
                    })(Data.AssetType || (Data.AssetType = {}));
                    var AssetType = Data.AssetType;
                })(VideoPlayer.Data || (VideoPlayer.Data = {}));
                var Data = VideoPlayer.Data;
            })(MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
            var VideoPlayer = MediaServices.VideoPlayer;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-videoplayer-data.js.map
