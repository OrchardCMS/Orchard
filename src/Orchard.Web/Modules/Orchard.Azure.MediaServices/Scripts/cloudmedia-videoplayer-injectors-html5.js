/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/underscore.d.ts" />
/// <reference path="typings/uri.d.ts" />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var VideoPlayer;
            (function (VideoPlayer) {
                var Injectors;
                (function (Injectors) {
                    var Html5Injector = (function (_super) {
                        __extends(Html5Injector, _super);
                        function Html5Injector() {
                            _super.apply(this, arguments);
                        }
                        Html5Injector.prototype.isSupported = function () {
                            var videoElement = document.createElement("video");
                            var result = videoElement && !!videoElement.canPlayType;
                            this.debug("Browser supports HTML5 video: {0}", result);
                            this.debug("isSupported() returns {0}.", result);
                            return result;
                        };
                        Html5Injector.prototype.inject = function () {
                            var _this = this;
                            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();
                            this.debug("Injecting player into element '{0}'.", this.container.id);
                            var videoElement = $("<video controls>").attr("width", this.playerWidth).attr("height", this.playerHeight);
                            if (firstThumbnailAsset)
                                videoElement.attr("poster", firstThumbnailAsset.MainFileUrl);
                            if (this.autoPlay)
                                videoElement.attr("autoplay", "");
                            var sourceElements = [];
                            // Adaptive streaming URLs from dynamic assets.
                            _(this.assetData.DynamicVideoAssets).forEach(function (asset) {
                                var smoothStreamingSourceElement = $("<source>").attr("src", asset.SmoothStreamingUrl).attr("type", "application/vnd.ms-sstr+xml");
                                var hlsSourceElement = $("<source>").attr("src", asset.HlsUrl).attr("type", "application/x-mpegURL");
                                var mpegDashSourceElement = $("<source>").attr("src", asset.MpegDashUrl).attr("type", "application/dash+xml");
                                if (_this.applyMediaQueries && asset.MediaQuery)
                                    $([smoothStreamingSourceElement, hlsSourceElement, mpegDashSourceElement]).attr("media", asset.MediaQuery);
                                sourceElements.push(smoothStreamingSourceElement, hlsSourceElement, mpegDashSourceElement);
                            });
                            // "Raw" asset video file URLs from dynamic assets (in decending bitrate order).
                            _(this.assetData.DynamicVideoAssets).forEach(function (asset) {
                                _((asset.EncoderMetadata && asset.EncoderMetadata.AssetFiles) || []).filter(function (assetFile) { return _(assetFile.VideoTracks).any(); }).sort(function (assetFile) { return assetFile.Bitrate; }).reverse().forEach(function (assetFile) {
                                    var url = new URI(asset.MainFileUrl).filename(assetFile.Name);
                                    var sourceElement = $("<source>").attr("src", url.toString()).attr("type", assetFile.MimeType);
                                    if (_this.applyMediaQueries && asset.MediaQuery)
                                        sourceElement.attr("media", asset.MediaQuery);
                                    sourceElements.push(sourceElement);
                                });
                            });
                            // Asset file URLs from non-dynamic assets.
                            _(this.assetData.VideoAssets).forEach(function (asset) {
                                var sourceElement = $("<source>").attr("src", asset.MainFileUrl).attr("type", asset.MimeType);
                                if (_this.applyMediaQueries && asset.MediaQuery)
                                    sourceElement.attr("media", asset.MediaQuery);
                                sourceElements.push(sourceElement);
                            });
                            _(this.filteredAssets().SubtitleAssets).forEach(function (asset) {
                                var sourceElement = $("<track kind=\"captions\">").attr("label", asset.Name).attr("src", asset.MainFileUrl).attr("srclang", asset.Language);
                                sourceElements.push(sourceElement);
                            });
                            if (!_(sourceElements).any()) {
                                this.debug("No sources available; cleaning up container and faulting this injector.");
                                this.fault();
                                return;
                            }
                            $(sourceElements).each(function (index, elem) {
                                $(elem).appendTo(videoElement);
                            });
                            videoElement.appendTo(this.container);
                            var lastSource = _(sourceElements).last()[0];
                            var errorHandler = function (e) {
                                _this.debug("Error detected; cleaning up container and faulting this injector.");
                                // TODO: Be a little more selective here, don't fail on any error.
                                _this.fault();
                            };
                            lastSource.addEventListener("error", errorHandler, false);
                            videoElement.on("error", errorHandler);
                        };
                        Html5Injector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            _super.prototype.debug.call(this, "Html5Injector: " + message, args);
                        };
                        return Html5Injector;
                    })(Injectors.Injector);
                    Injectors.Html5Injector = Html5Injector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
