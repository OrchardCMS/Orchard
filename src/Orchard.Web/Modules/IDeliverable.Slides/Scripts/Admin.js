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
(function ($)
{
    $(function ()
    {
        $(".engine-picker").on("change", function (e)
        {
            var engine = $(this).val();

            $(".engine-settings-editor").hide();
            $("[data-engine='" + engine + "']").show();
        }).trigger("change");
    });
})(jQuery);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImFkbWluLXNsaWRlcy1wYXJ0LmpzIiwiYWRtaW4tc2xpZGVzaG93LXBhcnQuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDdEZBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6IkFkbWluLmpzIiwic291cmNlc0NvbnRlbnQiOlsiKGZ1bmN0aW9uICgkKVxyXG57XHJcbiAgICAvLyBTbGlkZXMgcHJldmlldy5cclxuICAgIHZhciBhcHBseUNzc1NjYWxlID0gZnVuY3Rpb24gKGVsZW1lbnQsIHNjYWxlLCB0cmFuc2xhdGUpXHJcbiAgICB7XHJcbiAgICAgICAgdmFyIGJyb3dzZXJQcmVmaXhlcyA9IFtcIlwiLCBcIi1tcy1cIiwgXCItbW96LVwiLCBcIi13ZWJraXQtXCIsIFwiLW8tXCJdLFxyXG4gICAgICAgICAgICBvZmZzZXQgPSAoKDEgLSBzY2FsZSkgKiA1MCkgKyBcIiVcIixcclxuICAgICAgICAgICAgc2NhbGVTdHJpbmcgPSAodHJhbnNsYXRlICE9PSBmYWxzZSA/IFwidHJhbnNsYXRlKC1cIiArIG9mZnNldCArIFwiLCAtXCIgKyBvZmZzZXQgKyBcIikgXCIgOiBcIlwiKSArIFwic2NhbGUoXCIgKyBzY2FsZSArIFwiLFwiICsgc2NhbGUgKyBcIilcIjtcclxuICAgICAgICAkKGJyb3dzZXJQcmVmaXhlcykuZWFjaChmdW5jdGlvbiAoKVxyXG4gICAgICAgIHtcclxuICAgICAgICAgICAgZWxlbWVudFxyXG4gICAgICAgICAgICAgICAgLmNzcyh0aGlzICsgXCJ0cmFuc2Zvcm1cIiwgc2NhbGVTdHJpbmcpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIGVsZW1lbnRcclxuICAgICAgICAgICAgLmRhdGEoeyBzY2FsZTogc2NhbGUgfSlcclxuICAgICAgICAgICAgLmFkZENsYXNzKFwic2NhbGVkXCIpO1xyXG4gICAgfTtcclxuXHJcbiAgICB2YXIgc2NhbGVTbGlkZXMgPSBmdW5jdGlvbiAoKVxyXG4gICAge1xyXG4gICAgICAgICQoXCIuc2xpZGVzXCIpXHJcbiAgICAgICAgICAgIC5jc3MoXCJkaXNwbGF5XCIsIFwiYmxvY2tcIilcclxuICAgICAgICAgICAgLmVhY2goZnVuY3Rpb24gKClcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgdmFyIHNsaWRlc2hvdyA9ICQodGhpcyksXHJcbiAgICAgICAgICAgICAgICAgICAgc2xpZGUgPSBzbGlkZXNob3cuZmluZChcIi5zbGlkZS1wcmV2aWV3XCIpLFxyXG4gICAgICAgICAgICAgICAgICAgIHBhcmVudCA9IHNsaWRlLnBhcmVudCgpLFxyXG4gICAgICAgICAgICAgICAgICAgIHdpZHRoID0gMTUwLFxyXG4gICAgICAgICAgICAgICAgICAgIGhlaWdodCA9IDE1MCxcclxuICAgICAgICAgICAgICAgICAgICBib3VuZGluZ0RpbWVuc2lvbiA9IDE1MCxcclxuICAgICAgICAgICAgICAgICAgICBzbGlkZVN0eWxlID0gc2xpZGUuYXR0cihcInN0eWxlXCIpO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmICgoc2xpZGVTdHlsZSAhPSBudWxsICYmIHNsaWRlU3R5bGUuaW5kZXhPZihcIndpZHRoOlwiKSA9PSAtMSkpIHdpZHRoID0gMTAyNDtcclxuICAgICAgICAgICAgICAgIGlmICgoc2xpZGVTdHlsZSAhPSBudWxsICYmIHNsaWRlU3R5bGUuaW5kZXhPZihcImhlaWdodDpcIikgPT0gLTEpKSBoZWlnaHQgPSA3Njg7XHJcblxyXG4gICAgICAgICAgICAgICAgc2xpZGUuY3NzKHtcclxuICAgICAgICAgICAgICAgICAgICB3aWR0aDogd2lkdGggKyBcInB4XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgaGVpZ2h0OiBoZWlnaHQgKyBcInB4XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246IFwiYWJzb2x1dGVcIlxyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB2YXIgc2NhbGVkRm9yV2lkdGggPSB3aWR0aCA+IGhlaWdodCxcclxuICAgICAgICAgICAgICAgICAgICBsYXJnZXN0RGltZW5zaW9uID0gKHNjYWxlZEZvcldpZHRoID8gd2lkdGggOiBoZWlnaHQpLFxyXG4gICAgICAgICAgICAgICAgICAgIHNjYWxlID0gYm91bmRpbmdEaW1lbnNpb24gLyBsYXJnZXN0RGltZW5zaW9uO1xyXG5cclxuICAgICAgICAgICAgICAgIHBhcmVudC5jc3Moe1xyXG4gICAgICAgICAgICAgICAgICAgIHdpZHRoOiBNYXRoLmZsb29yKHdpZHRoICogc2NhbGUpICsgXCJweFwiLFxyXG4gICAgICAgICAgICAgICAgICAgIGhlaWdodDogTWF0aC5mbG9vcihoZWlnaHQgKiBzY2FsZSkgKyBcInB4XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246IFwicmVsYXRpdmVcIixcclxuICAgICAgICAgICAgICAgICAgICBvdmVyZmxvdzogXCJoaWRkZW5cIlxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICAgICAgYXBwbHlDc3NTY2FsZShzbGlkZSwgc2NhbGUpO1xyXG4gICAgICAgICAgICAgICAgc2xpZGVzaG93LnBhcmVudChcIi5zbGlkZXMtd3JhcHBlclwiKS5jc3MoXCJvdmVyZmxvd1wiLCBcInZpc2libGVcIik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgfTtcclxuXHJcbiAgICAkKHdpbmRvdykubG9hZChmdW5jdGlvbiAoKVxyXG4gICAge1xyXG4gICAgICAgIHNjYWxlU2xpZGVzKCk7XHJcbiAgICB9KTtcclxuXHJcbiAgICAvLyBTb3J0YWJsZSBzbGlkZXMuXHJcbiAgICAkKGZ1bmN0aW9uICgpXHJcbiAgICB7XHJcbiAgICAgICAgJChcIi5zbGlkZXMtd3JhcHBlci5pbnRlcmFjdGl2ZVwiKS5lYWNoKGZ1bmN0aW9uICgpXHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICB2YXIgd3JhcHBlciA9ICQodGhpcyk7XHJcblxyXG4gICAgICAgICAgICB2YXIgc2hvd0NoYW5nZWRNZXNzYWdlID0gZnVuY3Rpb24gKClcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgd3JhcHBlci5maW5kKFwiLmRpcnR5LW1lc3NhZ2VcIikuc2hvdygpO1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgdmFyIG9uU2xpZGVJbmRleENoYW5nZWQgPSBmdW5jdGlvbiAoZSwgdWkpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIHNob3dDaGFuZ2VkTWVzc2FnZSgpO1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgdmFyIHNsaWRlc0xpc3QgPSB3cmFwcGVyLmZpbmQoXCJ1bC5zbGlkZXNcIik7XHJcbiAgICAgICAgICAgIHNsaWRlc0xpc3Quc29ydGFibGUoe1xyXG4gICAgICAgICAgICAgICAgcGxhY2Vob2xkZXI6IFwic29ydGFibGUtcGxhY2Vob2xkZXJcIixcclxuICAgICAgICAgICAgICAgIHN0b3A6IG9uU2xpZGVJbmRleENoYW5nZWRcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIHNsaWRlc0xpc3QuZGlzYWJsZVNlbGVjdGlvbigpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfSk7XHJcbn0pKGpRdWVyeSk7IiwiKGZ1bmN0aW9uICgkKVxyXG57XHJcbiAgICAkKGZ1bmN0aW9uICgpXHJcbiAgICB7XHJcbiAgICAgICAgJChcIi5lbmdpbmUtcGlja2VyXCIpLm9uKFwiY2hhbmdlXCIsIGZ1bmN0aW9uIChlKVxyXG4gICAgICAgIHtcclxuICAgICAgICAgICAgdmFyIGVuZ2luZSA9ICQodGhpcykudmFsKCk7XHJcblxyXG4gICAgICAgICAgICAkKFwiLmVuZ2luZS1zZXR0aW5ncy1lZGl0b3JcIikuaGlkZSgpO1xyXG4gICAgICAgICAgICAkKFwiW2RhdGEtZW5naW5lPSdcIiArIGVuZ2luZSArIFwiJ11cIikuc2hvdygpO1xyXG4gICAgICAgIH0pLnRyaWdnZXIoXCJjaGFuZ2VcIik7XHJcbiAgICB9KTtcclxufSkoalF1ZXJ5KTsiXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=