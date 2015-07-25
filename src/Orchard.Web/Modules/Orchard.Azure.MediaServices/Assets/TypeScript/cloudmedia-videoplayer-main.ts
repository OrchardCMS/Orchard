/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />

module Orchard.Azure.MediaServices.VideoPlayer {

    import Data = Orchard.Azure.MediaServices.VideoPlayer.Data;
    import Injectors = Orchard.Azure.MediaServices.VideoPlayer.Injectors;

    $(function () {
        $(".cloudmedia-videoplayer-container").each(function (index, elem) {
            var container = <HTMLElement>elem;

            var assetData: Data.IAssetData = $(elem).data("cloudvideo-player-assetdata");
            var playerWidth: number = $(elem).data("cloudvideo-player-width");
            var playerHeight: number = $(elem).data("cloudvideo-player-height");
            var applyMediaQueries: boolean = $(elem).data("cloudvideo-player-applymediaqueries");
            var autoPlay: boolean = $(elem).data("cloudvideo-player-autoplay");
            var contentBaseUrl: string = $(elem).data("cloudvideo-player-content-baseurl");
            var errorText: string = $(elem).data("cloudvideo-player-errortext");
            var altText: string = $(elem).data("cloudvideo-player-alttext");
            var retryText: string = $(elem).data("cloudvideo-player-retrytext");

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

                var firstInjector: Injectors.Injector = smpInjector;
                firstInjector.invoke();
            }

            invokeInjectors();
        });
    });
}