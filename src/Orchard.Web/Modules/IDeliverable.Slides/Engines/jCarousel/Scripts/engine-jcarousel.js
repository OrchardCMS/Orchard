(function ($) {

    $(".jcarousel-engine").each(function () {
        var engine = $(this);
        var autoStart = engine.data("autostart");
        var interval = engine.data("interval") || 3000;
        var transitions = engine.data("transitions");
        var easing = engine.data("easing");
        var wrap = engine.data("wrap");
        var vertical = engine.data("vertical");
        var center = engine.data("center");

        engine.find(".jcarousel").jcarousel({
            wrap: wrap,
            vertical: vertical,
            center: center,
            transitions: transitions ? {
                transforms: Modernizr.csstransforms,
                transforms3d: Modernizr.csstransforms3d,
                easing: easing
            } : false
        })
        .jcarouselAutoscroll({
            interval: interval,
            target: "+=1",
            autostart: autoStart
        });

        engine.find(".jcarousel-prev").jcarouselControl({
            target: "-=1"
        });

        engine.find(".jcarousel-next").jcarouselControl({
            target: "+=1"
        });

        engine.find(".jcarousel-pagination").jcarouselPagination({
            item: function (page) {
                return "<a href=\"#" + page + "\">" + page + "</a>";
            }
        });
    });

})(jQuery);