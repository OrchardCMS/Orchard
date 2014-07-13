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
                    var DashInjector = (function (_super) {
                        __extends(DashInjector, _super);
                        function DashInjector() {
                            _super.apply(this, arguments);
                        }
                        DashInjector.prototype.isSupported = function () {
                            var videoElement = document.createElement("video");
                            var mse = window["MediaSource"] || window["WebKitMediaSource"];

                            var hasH264 = videoElement && videoElement.canPlayType && !!videoElement.canPlayType("video/mp4; codecs=\"avc1.42001E, mp4a.40.2\"");
                            var hasMse = mse && mse.isTypeSupported && mse.isTypeSupported("video/mp4; codecs=\"avc1.4d404f\"");
                            var hasDynamicAssets = _(this.filteredAssets().DynamicVideoAssets).any();

                            this.debug("Browser supports HTML5 video and the H264 and AAC codecs: {0}", hasH264);
                            this.debug("Browser supports the Media Source Extensions API: {0}", hasMse);
                            this.debug("Item has at least one dynamic video asset: {0}", hasDynamicAssets);

                            var result = hasH264 && hasMse && hasDynamicAssets;
                            this.debug("isSupported() returns {0}.", result);

                            return result;
                        };

                        DashInjector.prototype.inject = function () {
                            var _this = this;
                            var firstDynamicAsset = _(this.filteredAssets().DynamicVideoAssets).first();
                            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();

                            this.debug("Injecting player into element '{0}'.", this.container.id);

                            var videoElement = $("<video controls>").attr("width", this.playerWidth).attr("height", this.playerHeight);
                            if (firstThumbnailAsset)
                                videoElement.attr("poster", firstThumbnailAsset.MainFileUrl);
                            videoElement.appendTo(this.container);

                            var url = firstDynamicAsset.MpegDashUrl;
                            var context = new Dash.di.DashContext();
                            var player = new MediaPlayer(context);
                            player.startup();

                            player.addEventListener("error", function (e) {
                                _this.debug("Error of type '{0}' detected; cleaning up container and faulting this injector.", e.error);

                                // TODO: Be a little more selective here, don't fail on any error.
                                _this.fault();
                            });

                            player.debug.setLogToBrowserConsole(false);
                            player.attachView(videoElement[0]);
                            player.attachSource(url);
                            player.setAutoPlay(this.autoPlay);
                        };

                        DashInjector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 0; _i < (arguments.length - 1); _i++) {
                                args[_i] = arguments[_i + 1];
                            }
                            _super.prototype.debug.call(this, "DashInjector: " + message, args);
                        };
                        return DashInjector;
                    })(Orchard.Azure.MediaServices.VideoPlayer.Injectors.Injector);
                    Injectors.DashInjector = DashInjector;
                })(VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
                var Injectors = VideoPlayer.Injectors;
            })(MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
            var VideoPlayer = MediaServices.VideoPlayer;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-videoplayer-injectors-dash.js.map
