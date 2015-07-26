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
                    var DashInjector = (function (_super) {
                        __extends(DashInjector, _super);
                        function DashInjector() {
                            _super.apply(this, arguments);
                        }
                        DashInjector.prototype.isSupported = function () {
                            var videoElement = document.createElement("video");
                            var hasH264 = videoElement && videoElement.canPlayType && !!videoElement.canPlayType("video/mp4; codecs=\"avc1.42001E, mp4a.40.2\"");
                            var hasMse = MediaSource && MediaSource.isTypeSupported && MediaSource.isTypeSupported("video/mp4; codecs=\"avc1.4d404f\"");
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
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            _super.prototype.debug.call(this, "DashInjector: " + message, args);
                        };
                        return DashInjector;
                    })(Injectors.Injector);
                    Injectors.DashInjector = DashInjector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLWRhc2gudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5EYXNoSW5qZWN0b3IiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkRhc2hJbmplY3Rvci5jb25zdHJ1Y3RvciIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuRGFzaEluamVjdG9yLmlzU3VwcG9ydGVkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5EYXNoSW5qZWN0b3IuaW5qZWN0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5EYXNoSW5qZWN0b3IuZGVidWciXSwibWFwcGluZ3MiOiJBQUFBLDRDQUE0QztBQUM1QyxnREFBZ0Q7Ozs7Ozs7QUFFaEQsSUFBTyxPQUFPLENBcUViO0FBckVELFdBQU8sT0FBTztJQUFDQSxJQUFBQSxLQUFLQSxDQXFFbkJBO0lBckVjQSxXQUFBQSxLQUFLQTtRQUFDQyxJQUFBQSxhQUFhQSxDQXFFakNBO1FBckVvQkEsV0FBQUEsYUFBYUE7WUFBQ0MsSUFBQUEsV0FBV0EsQ0FxRTdDQTtZQXJFa0NBLFdBQUFBLFdBQVdBO2dCQUFDQyxJQUFBQSxTQUFTQSxDQXFFdkRBO2dCQXJFOENBLFdBQUFBLFNBQVNBLEVBQUNBLENBQUNBO29CQWtCdERDO3dCQUFrQ0MsZ0NBQVFBO3dCQUExQ0E7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QUFMQSxhQUFLQSxRQXFFbkJBO0FBQURBLENBQUNBLEVBckVNLE9BQU8sS0FBUCxPQUFPLFFBcUViIiwiZmlsZSI6ImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLWRhc2guanMiLCJzb3VyY2VzQ29udGVudCI6WyIvLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnkuZC50c1wiIC8+XHJcbi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL3VuZGVyc2NvcmUuZC50c1wiIC8+XHJcblxyXG5tb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycyB7XHJcblxyXG4gICAgaW1wb3J0IERhdGEgPSBPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuRGF0YTtcclxuXHJcbiAgICBkZWNsYXJlIHZhciBEYXNoOiBhbnk7XHJcbiAgICBkZWNsYXJlIHZhciBNZWRpYVBsYXllcjogYW55O1xyXG5cclxuICAgIGludGVyZmFjZSBQbGF5ZXJFcnJvckV2ZW50QXJncyB7XHJcbiAgICAgICAgdHlwZTogc3RyaW5nO1xyXG4gICAgICAgIGVycm9yOiBzdHJpbmc7XHJcbiAgICAgICAgZXZlbnQ6IHtcclxuICAgICAgICAgICAgaWQ/OiBzdHJpbmc7XHJcbiAgICAgICAgICAgIG1lc3NhZ2U/OiBzdHJpbmc7XHJcbiAgICAgICAgICAgIHJlcXVlc3Q/OiBYTUxIdHRwUmVxdWVzdDtcclxuICAgICAgICAgICAgbWFuaWZlc3Q/OiBhbnk7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBEYXNoSW5qZWN0b3IgZXh0ZW5kcyBJbmplY3RvciB7XHJcblxyXG4gICAgICAgIHB1YmxpYyBpc1N1cHBvcnRlZCgpOiBib29sZWFuIHtcclxuICAgICAgICAgICAgdmFyIHZpZGVvRWxlbWVudDogSFRNTFZpZGVvRWxlbWVudCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ2aWRlb1wiKTtcclxuXHJcbiAgICAgICAgICAgIHZhciBoYXNIMjY0ID0gdmlkZW9FbGVtZW50ICYmIHZpZGVvRWxlbWVudC5jYW5QbGF5VHlwZSAmJiAhIXZpZGVvRWxlbWVudC5jYW5QbGF5VHlwZShcInZpZGVvL21wNDsgY29kZWNzPVxcXCJhdmMxLjQyMDAxRSwgbXA0YS40MC4yXFxcIlwiKTtcclxuICAgICAgICAgICAgdmFyIGhhc01zZSA9IE1lZGlhU291cmNlICYmIE1lZGlhU291cmNlLmlzVHlwZVN1cHBvcnRlZCAmJiBNZWRpYVNvdXJjZS5pc1R5cGVTdXBwb3J0ZWQoXCJ2aWRlby9tcDQ7IGNvZGVjcz1cXFwiYXZjMS40ZDQwNGZcXFwiXCIpO1xyXG4gICAgICAgICAgICB2YXIgaGFzRHluYW1pY0Fzc2V0cyA9IF8odGhpcy5maWx0ZXJlZEFzc2V0cygpLkR5bmFtaWNWaWRlb0Fzc2V0cykuYW55KCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLmRlYnVnKFwiQnJvd3NlciBzdXBwb3J0cyBIVE1MNSB2aWRlbyBhbmQgdGhlIEgyNjQgYW5kIEFBQyBjb2RlY3M6IHswfVwiLCBoYXNIMjY0KTtcclxuICAgICAgICAgICAgdGhpcy5kZWJ1ZyhcIkJyb3dzZXIgc3VwcG9ydHMgdGhlIE1lZGlhIFNvdXJjZSBFeHRlbnNpb25zIEFQSTogezB9XCIsIGhhc01zZSk7XHJcbiAgICAgICAgICAgIHRoaXMuZGVidWcoXCJJdGVtIGhhcyBhdCBsZWFzdCBvbmUgZHluYW1pYyB2aWRlbyBhc3NldDogezB9XCIsIGhhc0R5bmFtaWNBc3NldHMpO1xyXG5cclxuICAgICAgICAgICAgdmFyIHJlc3VsdCA9IGhhc0gyNjQgJiYgaGFzTXNlICYmIGhhc0R5bmFtaWNBc3NldHM7XHJcbiAgICAgICAgICAgIHRoaXMuZGVidWcoXCJpc1N1cHBvcnRlZCgpIHJldHVybnMgezB9LlwiLCByZXN1bHQpO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIHJlc3VsdDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHB1YmxpYyBpbmplY3QoKTogdm9pZCB7XHJcbiAgICAgICAgICAgIHZhciBmaXJzdER5bmFtaWNBc3NldCA9IF8odGhpcy5maWx0ZXJlZEFzc2V0cygpLkR5bmFtaWNWaWRlb0Fzc2V0cykuZmlyc3QoKTtcclxuICAgICAgICAgICAgdmFyIGZpcnN0VGh1bWJuYWlsQXNzZXQgPSBfKHRoaXMuZmlsdGVyZWRBc3NldHMoKS5UaHVtYm5haWxBc3NldHMpLmZpcnN0KCk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLmRlYnVnKFwiSW5qZWN0aW5nIHBsYXllciBpbnRvIGVsZW1lbnQgJ3swfScuXCIsIHRoaXMuY29udGFpbmVyLmlkKTtcclxuXHJcbiAgICAgICAgICAgIHZhciB2aWRlb0VsZW1lbnQgPSAkKFwiPHZpZGVvIGNvbnRyb2xzPlwiKS5hdHRyKFwid2lkdGhcIiwgdGhpcy5wbGF5ZXJXaWR0aCkuYXR0cihcImhlaWdodFwiLCB0aGlzLnBsYXllckhlaWdodCk7XHJcbiAgICAgICAgICAgIGlmIChmaXJzdFRodW1ibmFpbEFzc2V0KVxyXG4gICAgICAgICAgICAgICAgdmlkZW9FbGVtZW50LmF0dHIoXCJwb3N0ZXJcIiwgZmlyc3RUaHVtYm5haWxBc3NldC5NYWluRmlsZVVybCk7XHJcbiAgICAgICAgICAgIHZpZGVvRWxlbWVudC5hcHBlbmRUbyh0aGlzLmNvbnRhaW5lcik7XHJcblxyXG4gICAgICAgICAgICB2YXIgdXJsID0gZmlyc3REeW5hbWljQXNzZXQuTXBlZ0Rhc2hVcmw7XHJcbiAgICAgICAgICAgIHZhciBjb250ZXh0ID0gbmV3IERhc2guZGkuRGFzaENvbnRleHQoKTtcclxuICAgICAgICAgICAgdmFyIHBsYXllciA9IG5ldyBNZWRpYVBsYXllcihjb250ZXh0KTtcclxuICAgICAgICAgICAgcGxheWVyLnN0YXJ0dXAoKTtcclxuXHJcbiAgICAgICAgICAgIHBsYXllci5hZGRFdmVudExpc3RlbmVyKFwiZXJyb3JcIiwgKGU6IFBsYXllckVycm9yRXZlbnRBcmdzKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmRlYnVnKFwiRXJyb3Igb2YgdHlwZSAnezB9JyBkZXRlY3RlZDsgY2xlYW5pbmcgdXAgY29udGFpbmVyIGFuZCBmYXVsdGluZyB0aGlzIGluamVjdG9yLlwiLCBlLmVycm9yKTtcclxuICAgICAgICAgICAgICAgIC8vIFRPRE86IEJlIGEgbGl0dGxlIG1vcmUgc2VsZWN0aXZlIGhlcmUsIGRvbid0IGZhaWwgb24gYW55IGVycm9yLlxyXG4gICAgICAgICAgICAgICAgdGhpcy5mYXVsdCgpO1xyXG4gICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIHBsYXllci5kZWJ1Zy5zZXRMb2dUb0Jyb3dzZXJDb25zb2xlKGZhbHNlKTtcclxuICAgICAgICAgICAgcGxheWVyLmF0dGFjaFZpZXcodmlkZW9FbGVtZW50WzBdKTtcclxuICAgICAgICAgICAgcGxheWVyLmF0dGFjaFNvdXJjZSh1cmwpO1xyXG4gICAgICAgICAgICBwbGF5ZXIuc2V0QXV0b1BsYXkodGhpcy5hdXRvUGxheSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgZGVidWcobWVzc2FnZTogc3RyaW5nLCAuLi5hcmdzOiBhbnlbXSk6IHZvaWQge1xyXG4gICAgICAgICAgICBzdXBlci5kZWJ1ZyhcIkRhc2hJbmplY3RvcjogXCIgKyBtZXNzYWdlLCBhcmdzKTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcbn0gIl0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9