/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/underscore.d.ts" />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (VideoPlayer) {
                (function (Injectors) {
                    Injectors.instances = new Array();

                    var SmpInjector = (function (_super) {
                        __extends(SmpInjector, _super);
                        function SmpInjector(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, debugToConsole, nextInjector, contentBaseUrl) {
                            _super.call(this, container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, debugToConsole, nextInjector);
                            this.contentBaseUrl = contentBaseUrl;
                            this.flashVersion = "10.2.0";
                            this.innerElementId = container.id + "-inner";
                        }
                        SmpInjector.prototype.isSupported = function () {
                            var browserHasFlash = swfobject.hasFlashPlayerVersion(this.flashVersion);
                            var hasDynamicAssets = _(this.filteredAssets().DynamicVideoAssets).any();
                            var result = browserHasFlash && hasDynamicAssets;

                            this.debug("Browser has required Flash version: {0}", browserHasFlash);
                            this.debug("Item has at least one dynamic video asset: {0}", hasDynamicAssets);

                            this.debug("isSupported() returns {0}.", result);
                            return result;
                        };

                        SmpInjector.prototype.inject = function () {
                            var _this = this;
                            var firstDynamicAsset = _(this.filteredAssets().DynamicVideoAssets).first();
                            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();

                            var flashvars = {
                                src: firstDynamicAsset.SmoothStreamingUrl,
                                plugin_AdaptiveStreamingPlugin: encodeURIComponent(this.contentBaseUrl + "MSAdaptiveStreamingPlugin.swf"),
                                AdaptiveStreamingPlugin_retryLive: "true",
                                AdaptiveStreamingPlugin_retryInterval: "10",
                                autoPlay: this.autoPlay.toString(),
                                bufferingOverlay: "false",
                                poster: firstThumbnailAsset ? encodeURIComponent(firstThumbnailAsset.MainFileUrl) : null,
                                javascriptCallbackFunction: "Orchard.Azure.MediaServices.VideoPlayer.Injectors.onSmpBridgeCreated"
                            };

                            var params = {
                                allowFullScreen: "true",
                                allowScriptAccess: "always",
                                wmode: "direct"
                            };

                            var attributes = {
                                id: this.innerElementId
                            };

                            $("<div>").attr("id", this.innerElementId).appendTo(this.container);
                            this.debug("Injecting player into element '{0}'.", this.container.id);

                            swfobject.embedSWF(this.contentBaseUrl + "StrobeMediaPlayback.swf", this.innerElementId, this.playerWidth.toString(), this.playerHeight.toString(), this.flashVersion, this.contentBaseUrl + "expressInstall.swf", flashvars, params, attributes, function (e) {
                                if (!e.success)
                                    _this.fault();
                            });

                            Injectors.instances[this.innerElementId] = this;
                        };

                        SmpInjector.prototype.onMediaPlayerStateChange = function (state) {
                            if (state == "playbackError") {
                                this.debug("Playback error detected; cleaning up container and faulting this injector.");
                                Injectors.instances[this.innerElementId] = null;
                                this.fault();
                            }
                        };

                        SmpInjector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 0; _i < (arguments.length - 1); _i++) {
                                args[_i] = arguments[_i + 1];
                            }
                            _super.prototype.debug.call(this, "SmpInjector: " + message, args);
                        };
                        return SmpInjector;
                    })(Orchard.Azure.MediaServices.VideoPlayer.Injectors.Injector);
                    Injectors.SmpInjector = SmpInjector;

                    function onSmpBridgeCreated(playerElementId) {
                        var player = document.getElementById(playerElementId);
                        if (player) {
                            player.addEventListener("mediaPlayerStateChange", "Orchard.Azure.MediaServices.VideoPlayer.Injectors.instances[\"" + playerElementId + "\"].onMediaPlayerStateChange");
                        }
                    }
                    Injectors.onSmpBridgeCreated = onSmpBridgeCreated;
                })(VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
                var Injectors = VideoPlayer.Injectors;
            })(MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
            var VideoPlayer = MediaServices.VideoPlayer;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-videoplayer-injectors-smp.js.map
