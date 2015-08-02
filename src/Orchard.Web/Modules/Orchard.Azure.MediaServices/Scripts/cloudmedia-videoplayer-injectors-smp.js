/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />
var __extends = (this && this.__extends) || function (d, b) {
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
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            _super.prototype.debug.call(this, "SmpInjector: " + message, args);
                        };
                        return SmpInjector;
                    })(Injectors.Injector);
                    Injectors.SmpInjector = SmpInjector;
                    function onSmpBridgeCreated(playerElementId) {
                        var player = document.getElementById(playerElementId);
                        if (player) {
                            player.addEventListener("mediaPlayerStateChange", "Orchard.Azure.MediaServices.VideoPlayer.Injectors.instances[\"" + playerElementId + "\"].onMediaPlayerStateChange");
                        }
                    }
                    Injectors.onSmpBridgeCreated = onSmpBridgeCreated;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLXNtcC50cyJdLCJuYW1lcyI6WyJPcmNoYXJkIiwiT3JjaGFyZC5BenVyZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllciIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLlNtcEluamVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5TbXBJbmplY3Rvci5jb25zdHJ1Y3RvciIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuU21wSW5qZWN0b3IuaXNTdXBwb3J0ZWQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLlNtcEluamVjdG9yLmluamVjdCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuU21wSW5qZWN0b3Iub25NZWRpYVBsYXllclN0YXRlQ2hhbmdlIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5TbXBJbmplY3Rvci5kZWJ1ZyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMub25TbXBCcmlkZ2VDcmVhdGVkIl0sIm1hcHBpbmdzIjoiQUFBQSw0Q0FBNEM7QUFDNUMsZ0RBQWdEOzs7Ozs7O0FBRWhELElBQU8sT0FBTyxDQXdHYjtBQXhHRCxXQUFPLE9BQU87SUFBQ0EsSUFBQUEsS0FBS0EsQ0F3R25CQTtJQXhHY0EsV0FBQUEsS0FBS0E7UUFBQ0MsSUFBQUEsYUFBYUEsQ0F3R2pDQTtRQXhHb0JBLFdBQUFBLGFBQWFBO1lBQUNDLElBQUFBLFdBQVdBLENBd0c3Q0E7WUF4R2tDQSxXQUFBQSxXQUFXQTtnQkFBQ0MsSUFBQUEsU0FBU0EsQ0F3R3ZEQTtnQkF4RzhDQSxXQUFBQSxTQUFTQSxFQUFDQSxDQUFDQTtvQkFNM0NDLG1CQUFTQSxHQUFrQkEsSUFBSUEsS0FBS0EsRUFBRUEsQ0FBQ0E7b0JBRWxEQTt3QkFBaUNDLCtCQUFRQTt3QkFLckNBLHFCQUNJQSxTQUFzQkEsRUFDdEJBLFdBQW1CQSxFQUNuQkEsWUFBb0JBLEVBQ3BCQSxRQUFpQkEsRUFDakJBLFNBQTBCQSxFQUMxQkEsaUJBQTBCQSxFQUMxQkEsY0FBdUJBLEVBQ3ZCQSxZQUFzQkEsRUFDZEEsY0FBc0JBOzRCQUM5QkMsa0JBQU1BLFNBQVNBLEVBQUVBLFdBQVdBLEVBQUVBLFlBQVlBLEVBQUVBLFFBQVFBLEVBQUVBLFNBQVNBLEVBQUVBLGlCQUFpQkEsRUFBRUEsY0FBY0EsRUFBRUEsWUFBWUEsQ0FBQ0EsQ0FBQ0E7NEJBRDFHQSxtQkFBY0EsR0FBZEEsY0FBY0EsQ0FBUUE7NEJBWjFCQSxpQkFBWUEsR0FBR0EsUUFBUUEsQ0FBQ0E7NEJBYzVCQSxJQUFJQSxDQUFDQSxjQUFjQSxHQUFHQSxTQUFTQSxDQUFDQSxFQUFFQSxHQUFHQSxRQUFRQSxDQUFDQTt3QkFDbERBLENBQUNBO3dCQUVNRCxpQ0FBV0EsR0FBbEJBOzRCQUNJRSxJQUFJQSxlQUFlQSxHQUFHQSxTQUFTQSxDQUFDQSxxQkFBcUJBLENBQUNBLElBQUlBLENBQUNBLFlBQVlBLENBQUNBLENBQUNBOzRCQUN6RUEsSUFBSUEsZ0JBQWdCQSxHQUFHQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxjQUFjQSxFQUFFQSxDQUFDQSxrQkFBa0JBLENBQUNBLENBQUNBLEdBQUdBLEVBQUVBLENBQUNBOzRCQUN6RUEsSUFBSUEsTUFBTUEsR0FBR0EsZUFBZUEsSUFBSUEsZ0JBQWdCQSxDQUFDQTs0QkFFakRBLElBQUlBLENBQUNBLEtBQUtBLENBQUNBLHlDQUF5Q0EsRUFBRUEsZUFBZUEsQ0FBQ0EsQ0FBQ0E7NEJBQ3ZFQSxJQUFJQSxDQUFDQSxLQUFLQSxDQUFDQSxnREFBZ0RBLEVBQUVBLGdCQUFnQkEsQ0FBQ0EsQ0FBQ0E7NEJBRS9FQSxJQUFJQSxDQUFDQSxLQUFLQSxDQUFDQSw0QkFBNEJBLEVBQUVBLE1BQU1BLENBQUNBLENBQUNBOzRCQUNqREEsTUFBTUEsQ0FBQ0EsTUFBTUEsQ0FBQ0E7d0JBQ2xCQSxDQUFDQTt3QkFFTUYsNEJBQU1BLEdBQWJBOzRCQUFBRyxpQkE0Q0NBOzRCQTNDR0EsSUFBSUEsaUJBQWlCQSxHQUFHQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxjQUFjQSxFQUFFQSxDQUFDQSxrQkFBa0JBLENBQUNBLENBQUNBLEtBQUtBLEVBQUVBLENBQUNBOzRCQUM1RUEsSUFBSUEsbUJBQW1CQSxHQUFHQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxjQUFjQSxFQUFFQSxDQUFDQSxlQUFlQSxDQUFDQSxDQUFDQSxLQUFLQSxFQUFFQSxDQUFDQTs0QkFFM0VBLElBQUlBLFNBQVNBLEdBQUdBO2dDQUNaQSxHQUFHQSxFQUFFQSxpQkFBaUJBLENBQUNBLGtCQUFrQkE7Z0NBQ3pDQSw4QkFBOEJBLEVBQUVBLGtCQUFrQkEsQ0FBQ0EsSUFBSUEsQ0FBQ0EsY0FBY0EsR0FBR0EsK0JBQStCQSxDQUFDQTtnQ0FDekdBLGlDQUFpQ0EsRUFBRUEsTUFBTUE7Z0NBQ3pDQSxxQ0FBcUNBLEVBQUVBLElBQUlBO2dDQUMzQ0EsUUFBUUEsRUFBRUEsSUFBSUEsQ0FBQ0EsUUFBUUEsQ0FBQ0EsUUFBUUEsRUFBRUE7Z0NBQ2xDQSxnQkFBZ0JBLEVBQUVBLE9BQU9BO2dDQUN6QkEsTUFBTUEsRUFBRUEsbUJBQW1CQSxHQUFHQSxrQkFBa0JBLENBQUNBLG1CQUFtQkEsQ0FBQ0EsV0FBV0EsQ0FBQ0EsR0FBR0EsSUFBSUE7Z0NBQ3hGQSwwQkFBMEJBLEVBQUVBLHNFQUFzRUE7NkJBQ3JHQSxDQUFDQTs0QkFFRkEsSUFBSUEsTUFBTUEsR0FBR0E7Z0NBQ1RBLGVBQWVBLEVBQUVBLE1BQU1BO2dDQUN2QkEsaUJBQWlCQSxFQUFFQSxRQUFRQTtnQ0FDM0JBLEtBQUtBLEVBQUVBLFFBQVFBOzZCQUNsQkEsQ0FBQ0E7NEJBRUZBLElBQUlBLFVBQVVBLEdBQUdBO2dDQUNiQSxFQUFFQSxFQUFFQSxJQUFJQSxDQUFDQSxjQUFjQTs2QkFDMUJBLENBQUNBOzRCQUVGQSxDQUFDQSxDQUFDQSxPQUFPQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxJQUFJQSxFQUFFQSxJQUFJQSxDQUFDQSxjQUFjQSxDQUFDQSxDQUFDQSxRQUFRQSxDQUFDQSxJQUFJQSxDQUFDQSxTQUFTQSxDQUFDQSxDQUFDQTs0QkFDcEVBLElBQUlBLENBQUNBLEtBQUtBLENBQUNBLHNDQUFzQ0EsRUFBRUEsSUFBSUEsQ0FBQ0EsU0FBU0EsQ0FBQ0EsRUFBRUEsQ0FBQ0EsQ0FBQ0E7NEJBRXRFQSxTQUFTQSxDQUFDQSxRQUFRQSxDQUNkQSxJQUFJQSxDQUFDQSxjQUFjQSxHQUFHQSx5QkFBeUJBLEVBQy9DQSxJQUFJQSxDQUFDQSxjQUFjQSxFQUNuQkEsSUFBSUEsQ0FBQ0EsV0FBV0EsQ0FBQ0EsUUFBUUEsRUFBRUEsRUFDM0JBLElBQUlBLENBQUNBLFlBQVlBLENBQUNBLFFBQVFBLEVBQUVBLEVBQzVCQSxJQUFJQSxDQUFDQSxZQUFZQSxFQUNqQkEsSUFBSUEsQ0FBQ0EsY0FBY0EsR0FBR0Esb0JBQW9CQSxFQUMxQ0EsU0FBU0EsRUFDVEEsTUFBTUEsRUFDTkEsVUFBVUEsRUFDVkEsVUFBQUEsQ0FBQ0E7Z0NBQ0dBLEVBQUVBLENBQUNBLENBQUNBLENBQUNBLENBQUNBLENBQUNBLE9BQU9BLENBQUNBO29DQUNYQSxLQUFJQSxDQUFDQSxLQUFLQSxFQUFFQSxDQUFDQTs0QkFDckJBLENBQUNBLENBQUNBLENBQUNBOzRCQUVQQSxtQkFBU0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EsY0FBY0EsQ0FBQ0EsR0FBR0EsSUFBSUEsQ0FBQ0E7d0JBQzFDQSxDQUFDQTt3QkFFTUgsOENBQXdCQSxHQUEvQkEsVUFBZ0NBLEtBQWFBOzRCQUN6Q0ksRUFBRUEsQ0FBQ0EsQ0FBQ0EsS0FBS0EsSUFBSUEsZUFBZUEsQ0FBQ0EsQ0FBQ0EsQ0FBQ0E7Z0NBQzNCQSxJQUFJQSxDQUFDQSxLQUFLQSxDQUFDQSw0RUFBNEVBLENBQUNBLENBQUNBO2dDQUN6RkEsbUJBQVNBLENBQUNBLElBQUlBLENBQUNBLGNBQWNBLENBQUNBLEdBQUdBLElBQUlBLENBQUNBO2dDQUN0Q0EsSUFBSUEsQ0FBQ0EsS0FBS0EsRUFBRUEsQ0FBQ0E7NEJBQ2pCQSxDQUFDQTt3QkFDTEEsQ0FBQ0E7d0JBRU1KLDJCQUFLQSxHQUFaQSxVQUFhQSxPQUFlQTs0QkFBRUssY0FBY0E7aUNBQWRBLFdBQWNBLENBQWRBLHNCQUFjQSxDQUFkQSxJQUFjQTtnQ0FBZEEsNkJBQWNBOzs0QkFDeENBLGdCQUFLQSxDQUFDQSxLQUFLQSxZQUFDQSxlQUFlQSxHQUFHQSxPQUFPQSxFQUFFQSxJQUFJQSxDQUFDQSxDQUFDQTt3QkFDakRBLENBQUNBO3dCQUNMTCxrQkFBQ0E7b0JBQURBLENBeEZBRCxBQXdGQ0MsRUF4RmdDRCxrQkFBUUEsRUF3RnhDQTtvQkF4RllBLHFCQUFXQSxjQXdGdkJBLENBQUFBO29CQUVEQSw0QkFBbUNBLGVBQXVCQTt3QkFDdERPLElBQUlBLE1BQU1BLEdBQUdBLFFBQVFBLENBQUNBLGNBQWNBLENBQUNBLGVBQWVBLENBQUNBLENBQUNBO3dCQUN0REEsRUFBRUEsQ0FBQ0EsQ0FBQ0EsTUFBTUEsQ0FBQ0EsQ0FBQ0EsQ0FBQ0E7NEJBQ0hBLE1BQU9BLENBQUNBLGdCQUFnQkEsQ0FBQ0Esd0JBQXdCQSxFQUFFQSxnRUFBZ0VBLEdBQUdBLGVBQWVBLEdBQUdBLDhCQUE4QkEsQ0FBQ0EsQ0FBQ0E7d0JBQ2xMQSxDQUFDQTtvQkFDTEEsQ0FBQ0E7b0JBTGVQLDRCQUFrQkEscUJBS2pDQSxDQUFBQTtnQkFDTEEsQ0FBQ0EsRUF4RzhDRCxTQUFTQSxHQUFUQSxxQkFBU0EsS0FBVEEscUJBQVNBLFFBd0d2REE7WUFBREEsQ0FBQ0EsRUF4R2tDRCxXQUFXQSxHQUFYQSx5QkFBV0EsS0FBWEEseUJBQVdBLFFBd0c3Q0E7UUFBREEsQ0FBQ0EsRUF4R29CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBd0dqQ0E7SUFBREEsQ0FBQ0EsRUF4R2NELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBd0duQkE7QUFBREEsQ0FBQ0EsRUF4R00sT0FBTyxLQUFQLE9BQU8sUUF3R2IiLCJmaWxlIjoiY2xvdWRtZWRpYS12aWRlb3BsYXllci1pbmplY3RvcnMtc21wLmpzIiwic291cmNlc0NvbnRlbnQiOlsiLy8vIDxyZWZlcmVuY2UgcGF0aD1cIlR5cGluZ3MvanF1ZXJ5LmQudHNcIiAvPlxyXG4vLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy91bmRlcnNjb3JlLmQudHNcIiAvPlxyXG5cclxubW9kdWxlIE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMge1xyXG5cclxuICAgIGltcG9ydCBEYXRhID0gT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkRhdGE7XHJcblxyXG4gICAgZGVjbGFyZSB2YXIgc3dmb2JqZWN0OiBhbnk7XHJcblxyXG4gICAgZXhwb3J0IHZhciBpbnN0YW5jZXM6IFNtcEluamVjdG9yW10gPSBuZXcgQXJyYXkoKTtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgU21wSW5qZWN0b3IgZXh0ZW5kcyBJbmplY3RvciB7XHJcblxyXG4gICAgICAgIHByaXZhdGUgZmxhc2hWZXJzaW9uID0gXCIxMC4yLjBcIjtcclxuICAgICAgICBwcml2YXRlIGlubmVyRWxlbWVudElkOiBzdHJpbmc7XHJcblxyXG4gICAgICAgIGNvbnN0cnVjdG9yKFxyXG4gICAgICAgICAgICBjb250YWluZXI6IEhUTUxFbGVtZW50LFxyXG4gICAgICAgICAgICBwbGF5ZXJXaWR0aDogbnVtYmVyLFxyXG4gICAgICAgICAgICBwbGF5ZXJIZWlnaHQ6IG51bWJlcixcclxuICAgICAgICAgICAgYXV0b1BsYXk6IGJvb2xlYW4sXHJcbiAgICAgICAgICAgIGFzc2V0RGF0YTogRGF0YS5JQXNzZXREYXRhLFxyXG4gICAgICAgICAgICBhcHBseU1lZGlhUXVlcmllczogYm9vbGVhbixcclxuICAgICAgICAgICAgZGVidWdUb0NvbnNvbGU6IGJvb2xlYW4sXHJcbiAgICAgICAgICAgIG5leHRJbmplY3RvcjogSW5qZWN0b3IsXHJcbiAgICAgICAgICAgIHByaXZhdGUgY29udGVudEJhc2VVcmw6IHN0cmluZykge1xyXG4gICAgICAgICAgICBzdXBlcihjb250YWluZXIsIHBsYXllcldpZHRoLCBwbGF5ZXJIZWlnaHQsIGF1dG9QbGF5LCBhc3NldERhdGEsIGFwcGx5TWVkaWFRdWVyaWVzLCBkZWJ1Z1RvQ29uc29sZSwgbmV4dEluamVjdG9yKTtcclxuICAgICAgICAgICAgdGhpcy5pbm5lckVsZW1lbnRJZCA9IGNvbnRhaW5lci5pZCArIFwiLWlubmVyXCI7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgaXNTdXBwb3J0ZWQoKTogYm9vbGVhbiB7XHJcbiAgICAgICAgICAgIHZhciBicm93c2VySGFzRmxhc2ggPSBzd2ZvYmplY3QuaGFzRmxhc2hQbGF5ZXJWZXJzaW9uKHRoaXMuZmxhc2hWZXJzaW9uKTtcclxuICAgICAgICAgICAgdmFyIGhhc0R5bmFtaWNBc3NldHMgPSBfKHRoaXMuZmlsdGVyZWRBc3NldHMoKS5EeW5hbWljVmlkZW9Bc3NldHMpLmFueSgpO1xyXG4gICAgICAgICAgICB2YXIgcmVzdWx0ID0gYnJvd3Nlckhhc0ZsYXNoICYmIGhhc0R5bmFtaWNBc3NldHM7XHJcblxyXG4gICAgICAgICAgICB0aGlzLmRlYnVnKFwiQnJvd3NlciBoYXMgcmVxdWlyZWQgRmxhc2ggdmVyc2lvbjogezB9XCIsIGJyb3dzZXJIYXNGbGFzaCk7XHJcbiAgICAgICAgICAgIHRoaXMuZGVidWcoXCJJdGVtIGhhcyBhdCBsZWFzdCBvbmUgZHluYW1pYyB2aWRlbyBhc3NldDogezB9XCIsIGhhc0R5bmFtaWNBc3NldHMpO1xyXG5cclxuICAgICAgICAgICAgdGhpcy5kZWJ1ZyhcImlzU3VwcG9ydGVkKCkgcmV0dXJucyB7MH0uXCIsIHJlc3VsdCk7XHJcbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgaW5qZWN0KCk6IHZvaWQge1xyXG4gICAgICAgICAgICB2YXIgZmlyc3REeW5hbWljQXNzZXQgPSBfKHRoaXMuZmlsdGVyZWRBc3NldHMoKS5EeW5hbWljVmlkZW9Bc3NldHMpLmZpcnN0KCk7XHJcbiAgICAgICAgICAgIHZhciBmaXJzdFRodW1ibmFpbEFzc2V0ID0gXyh0aGlzLmZpbHRlcmVkQXNzZXRzKCkuVGh1bWJuYWlsQXNzZXRzKS5maXJzdCgpO1xyXG5cclxuICAgICAgICAgICAgdmFyIGZsYXNodmFycyA9IHtcclxuICAgICAgICAgICAgICAgIHNyYzogZmlyc3REeW5hbWljQXNzZXQuU21vb3RoU3RyZWFtaW5nVXJsLFxyXG4gICAgICAgICAgICAgICAgcGx1Z2luX0FkYXB0aXZlU3RyZWFtaW5nUGx1Z2luOiBlbmNvZGVVUklDb21wb25lbnQodGhpcy5jb250ZW50QmFzZVVybCArIFwiTVNBZGFwdGl2ZVN0cmVhbWluZ1BsdWdpbi5zd2ZcIiksXHJcbiAgICAgICAgICAgICAgICBBZGFwdGl2ZVN0cmVhbWluZ1BsdWdpbl9yZXRyeUxpdmU6IFwidHJ1ZVwiLFxyXG4gICAgICAgICAgICAgICAgQWRhcHRpdmVTdHJlYW1pbmdQbHVnaW5fcmV0cnlJbnRlcnZhbDogXCIxMFwiLFxyXG4gICAgICAgICAgICAgICAgYXV0b1BsYXk6IHRoaXMuYXV0b1BsYXkudG9TdHJpbmcoKSxcclxuICAgICAgICAgICAgICAgIGJ1ZmZlcmluZ092ZXJsYXk6IFwiZmFsc2VcIixcclxuICAgICAgICAgICAgICAgIHBvc3RlcjogZmlyc3RUaHVtYm5haWxBc3NldCA/IGVuY29kZVVSSUNvbXBvbmVudChmaXJzdFRodW1ibmFpbEFzc2V0Lk1haW5GaWxlVXJsKSA6IG51bGwsXHJcbiAgICAgICAgICAgICAgICBqYXZhc2NyaXB0Q2FsbGJhY2tGdW5jdGlvbjogXCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLm9uU21wQnJpZGdlQ3JlYXRlZFwiXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgICBcclxuICAgICAgICAgICAgdmFyIHBhcmFtcyA9IHtcclxuICAgICAgICAgICAgICAgIGFsbG93RnVsbFNjcmVlbjogXCJ0cnVlXCIsXHJcbiAgICAgICAgICAgICAgICBhbGxvd1NjcmlwdEFjY2VzczogXCJhbHdheXNcIixcclxuICAgICAgICAgICAgICAgIHdtb2RlOiBcImRpcmVjdFwiXHJcbiAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICB2YXIgYXR0cmlidXRlcyA9IHtcclxuICAgICAgICAgICAgICAgIGlkOiB0aGlzLmlubmVyRWxlbWVudElkXHJcbiAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAkKFwiPGRpdj5cIikuYXR0cihcImlkXCIsIHRoaXMuaW5uZXJFbGVtZW50SWQpLmFwcGVuZFRvKHRoaXMuY29udGFpbmVyKTtcclxuICAgICAgICAgICAgdGhpcy5kZWJ1ZyhcIkluamVjdGluZyBwbGF5ZXIgaW50byBlbGVtZW50ICd7MH0nLlwiLCB0aGlzLmNvbnRhaW5lci5pZCk7XHJcblxyXG4gICAgICAgICAgICBzd2ZvYmplY3QuZW1iZWRTV0YoXHJcbiAgICAgICAgICAgICAgICB0aGlzLmNvbnRlbnRCYXNlVXJsICsgXCJTdHJvYmVNZWRpYVBsYXliYWNrLnN3ZlwiLFxyXG4gICAgICAgICAgICAgICAgdGhpcy5pbm5lckVsZW1lbnRJZCxcclxuICAgICAgICAgICAgICAgIHRoaXMucGxheWVyV2lkdGgudG9TdHJpbmcoKSxcclxuICAgICAgICAgICAgICAgIHRoaXMucGxheWVySGVpZ2h0LnRvU3RyaW5nKCksXHJcbiAgICAgICAgICAgICAgICB0aGlzLmZsYXNoVmVyc2lvbixcclxuICAgICAgICAgICAgICAgIHRoaXMuY29udGVudEJhc2VVcmwgKyBcImV4cHJlc3NJbnN0YWxsLnN3ZlwiLFxyXG4gICAgICAgICAgICAgICAgZmxhc2h2YXJzLFxyXG4gICAgICAgICAgICAgICAgcGFyYW1zLFxyXG4gICAgICAgICAgICAgICAgYXR0cmlidXRlcyxcclxuICAgICAgICAgICAgICAgIGUgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghZS5zdWNjZXNzKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmZhdWx0KCk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIGluc3RhbmNlc1t0aGlzLmlubmVyRWxlbWVudElkXSA9IHRoaXM7XHJcbiAgICAgICAgfVxyXG4gICAgICAgICBcclxuICAgICAgICBwdWJsaWMgb25NZWRpYVBsYXllclN0YXRlQ2hhbmdlKHN0YXRlOiBzdHJpbmcpIHtcclxuICAgICAgICAgICAgaWYgKHN0YXRlID09IFwicGxheWJhY2tFcnJvclwiKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmRlYnVnKFwiUGxheWJhY2sgZXJyb3IgZGV0ZWN0ZWQ7IGNsZWFuaW5nIHVwIGNvbnRhaW5lciBhbmQgZmF1bHRpbmcgdGhpcyBpbmplY3Rvci5cIik7XHJcbiAgICAgICAgICAgICAgICBpbnN0YW5jZXNbdGhpcy5pbm5lckVsZW1lbnRJZF0gPSBudWxsO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5mYXVsdCgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgZGVidWcobWVzc2FnZTogc3RyaW5nLCAuLi5hcmdzOiBhbnlbXSk6IHZvaWQge1xyXG4gICAgICAgICAgICBzdXBlci5kZWJ1ZyhcIlNtcEluamVjdG9yOiBcIiArIG1lc3NhZ2UsIGFyZ3MpO1xyXG4gICAgICAgIH1cclxuICAgIH1cclxuICAgICBcclxuICAgIGV4cG9ydCBmdW5jdGlvbiBvblNtcEJyaWRnZUNyZWF0ZWQocGxheWVyRWxlbWVudElkOiBzdHJpbmcpIHtcclxuICAgICAgICB2YXIgcGxheWVyID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQocGxheWVyRWxlbWVudElkKTtcclxuICAgICAgICBpZiAocGxheWVyKSB7XHJcbiAgICAgICAgICAgICg8YW55PnBsYXllcikuYWRkRXZlbnRMaXN0ZW5lcihcIm1lZGlhUGxheWVyU3RhdGVDaGFuZ2VcIiwgXCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLmluc3RhbmNlc1tcXFwiXCIgKyBwbGF5ZXJFbGVtZW50SWQgKyBcIlxcXCJdLm9uTWVkaWFQbGF5ZXJTdGF0ZUNoYW5nZVwiKTtcclxuICAgICAgICB9XHJcbiAgICB9IFxyXG59Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9