(function ($)
{
    // Slides preview.
    var applyCssScale = function (element, scale, translate)
    {
        var browserPrefixes = ["", "-ms-", "-moz-", "-webkit-", "-o-"],
            offset = ((1 - scale) * 50) + "%",
            scaleString = (translate !== false ? "translate(-" + offset + ", -" + offset + ") " : "") + "scale(" + scale + "," + scale + ")";
        $(browserPrefixes).each(function ()
        {
            element
                .css(this + "transform", scaleString);
        });
        element
            .data({ scale: scale })
            .addClass("scaled");
    };

    var scaleSlides = function ()
    {
        $(".slides")
            .css("display", "block")
            .each(function ()
            {
                var slideshow = $(this),
                    slide = slideshow.find(".slide-preview"),
                    parent = slide.parent(),
                    width = 150,
                    height = 150,
                    boundingDimension = 150,
                    slideStyle = slide.attr("style");

                if ((slideStyle != null && slideStyle.indexOf("width:") == -1)) width = 1024;
                if ((slideStyle != null && slideStyle.indexOf("height:") == -1)) height = 768;

                slide.css({
                    width: width + "px",
                    height: height + "px",
                    position: "absolute"
                });
                var scaledForWidth = width > height,
                    largestDimension = (scaledForWidth ? width : height),
                    scale = boundingDimension / largestDimension;

                parent.css({
                    width: Math.floor(width * scale) + "px",
                    height: Math.floor(height * scale) + "px",
                    position: "relative",
                    overflow: "hidden"
                });

                applyCssScale(slide, scale);
                slideshow.parent(".slides-wrapper").css("overflow", "visible");
            });
    };

    $(window).load(function ()
    {
        scaleSlides();
    });

    // Sortable slides.
    $(function ()
    {
        $(".slides-wrapper.interactive").each(function ()
        {
            var wrapper = $(this);

            var showChangedMessage = function ()
            {
                wrapper.find(".dirty-message").show();
            };

            var onSlideIndexChanged = function (e, ui)
            {
                showChangedMessage();
            };

            var slidesList = wrapper.find("ul.slides");
            slidesList.sortable({
                placeholder: "sortable-placeholder",
                stop: onSlideIndexChanged
            });
            slidesList.disableSelection();
        });
    });
})(jQuery);