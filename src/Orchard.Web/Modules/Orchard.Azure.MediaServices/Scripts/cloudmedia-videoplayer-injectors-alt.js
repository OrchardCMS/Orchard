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
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var VideoPlayer;
            (function (VideoPlayer) {
                var Injectors;
                (function (Injectors) {
                    var AltInjector = (function (_super) {
                        __extends(AltInjector, _super);
                        function AltInjector(container, playerWidth, playerHeight, assetData, applyMediaQueries, debugToConsole, nextInjector, alternateContent) {
                            _super.call(this, container, playerWidth, playerHeight, false, assetData, applyMediaQueries, debugToConsole, nextInjector);
                            this.alternateContent = alternateContent;
                        }
                        AltInjector.prototype.isSupported = function () {
                            return true;
                        };
                        AltInjector.prototype.inject = function () {
                            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();
                            this.debug("Injecting alternate content into element '{0}'.", this.container.id);
                            var wrapper = $("<div>").addClass("cloudvideo-player-alt-wrapper").css("width", this.playerWidth).css("height", this.playerHeight);
                            if (firstThumbnailAsset)
                                wrapper.css("background-image", "url('" + firstThumbnailAsset.MainFileUrl + "')");
                            var inner = $("<div>").addClass("cloudvideo-player-alt-inner").appendTo(wrapper);
                            if (this.alternateContent)
                                _(this.alternateContent).each(function (elem) {
                                    $(elem).appendTo(inner);
                                });
                            wrapper.appendTo(this.container);
                        };
                        AltInjector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            _super.prototype.debug.call(this, "AltInjector: " + message, args);
                        };
                        return AltInjector;
                    })(Injectors.Injector);
                    Injectors.AltInjector = AltInjector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
