/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/underscore.d.ts" />
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (VideoPlayer) {
                var Injectors = Orchard.Azure.MediaServices.VideoPlayer.Injectors;

                $(function () {
                    $(".cloudmedia-videoplayer-container").each(function (index, elem) {
                        var container = elem;

                        var assetData = $(elem).data("cloudvideo-player-assetdata");
                        var playerWidth = $(elem).data("cloudvideo-player-width");
                        var playerHeight = $(elem).data("cloudvideo-player-height");
                        var applyMediaQueries = $(elem).data("cloudvideo-player-applymediaqueries");
                        var autoPlay = $(elem).data("cloudvideo-player-autoplay");
                        var contentBaseUrl = $(elem).data("cloudvideo-player-content-baseurl");
                        var errorText = $(elem).data("cloudvideo-player-errortext");
                        var altText = $(elem).data("cloudvideo-player-alttext");
                        var retryText = $(elem).data("cloudvideo-player-retrytext");

                        function invokeInjectors() {
                            $(container).empty();

                            var alternateContent = [
                                $("<span>").addClass("cloudvideo-player-error-text").text(errorText),
                                $("<button>").addClass("cloudvideo-player-retry-button").text(retryText).click(function () {
                                    invokeInjectors();
                                }),
                                $("<span>").addClass("cloudvideo-player-alt-text").text(altText)
                            ];

                            // Construct a chain of injectors (each will invoke the next on failure).
                            var altInjector = new Injectors.AltInjector(container, playerWidth, playerHeight, assetData, applyMediaQueries, true, null, alternateContent);
                            var html5Injector = new Injectors.Html5Injector(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, true, altInjector);
                            var dashInjector = new Injectors.DashInjector(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, true, html5Injector);
                            var smpInjector = new Injectors.SmpInjector(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, true, dashInjector, contentBaseUrl);

                            var firstInjector = smpInjector;
                            firstInjector.invoke();
                        }

                        invokeInjectors();
                    });
                });
            })(MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
            var VideoPlayer = MediaServices.VideoPlayer;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-videoplayer-main.js.map
