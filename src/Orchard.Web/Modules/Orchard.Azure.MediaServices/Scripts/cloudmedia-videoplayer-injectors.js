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
                    var Injector = (function () {
                        function Injector(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, debugToConsole, nextInjector) {
                            this.container = container;
                            this.playerWidth = playerWidth;
                            this.playerHeight = playerHeight;
                            this.autoPlay = autoPlay;
                            this.assetData = assetData;
                            this.applyMediaQueries = applyMediaQueries;
                            this.debugToConsole = debugToConsole;
                            this.nextInjector = nextInjector;
                            this._isFaulted = false;
                        }
                        Injector.prototype.isFaulted = function () {
                            return this._isFaulted;
                        };
                        Injector.prototype.invoke = function () {
                            if (this.isSupported())
                                this.inject();
                            else if (this.nextInjector)
                                this.nextInjector.invoke();
                        };
                        Injector.prototype.isSupported = function () {
                            throw new Error("This method is abstract and must be overridden in an inherited class.");
                        };
                        Injector.prototype.inject = function () {
                            throw new Error("This method is abstract and must be overridden in an inherited class.");
                        };
                        Injector.prototype.filteredAssets = function () {
                            if (!this.applyMediaQueries)
                                return this.assetData;
                            var hasMatchingMediaQuery = function (asset) {
                                return !asset.MediaQuery || window.matchMedia(asset.MediaQuery).matches;
                            };
                            return {
                                VideoAssets: _(this.assetData.VideoAssets).filter(hasMatchingMediaQuery),
                                DynamicVideoAssets: _(this.assetData.DynamicVideoAssets).filter(hasMatchingMediaQuery),
                                ThumbnailAssets: _(this.assetData.ThumbnailAssets).filter(hasMatchingMediaQuery),
                                SubtitleAssets: _(this.assetData.SubtitleAssets).filter(hasMatchingMediaQuery)
                            };
                        };
                        Injector.prototype.fault = function () {
                            if (!this._isFaulted) {
                                this._isFaulted = true;
                                $(this.container).empty();
                                if (this.nextInjector)
                                    this.nextInjector.invoke();
                            }
                        };
                        Injector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            if (this.debugToConsole) {
                                console.debug(message.replace(/{(\d+)}/g, function (match, index) {
                                    return (typeof args[index] != "undefined" ? args[index] : match);
                                }));
                            }
                        };
                        return Injector;
                    })();
                    Injectors.Injector = Injector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
