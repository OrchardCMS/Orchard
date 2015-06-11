(function ($) {
    // Slides preview.
    var applyCssScale = function (element, scale, translate) {
        var browserPrefixes = ["", "-ms-", "-moz-", "-webkit-", "-o-"],
            offset = ((1 - scale) * 50) + "%",
            scaleString = (translate !== false ? "translate(-" + offset + ", -" + offset + ") " : "") + "scale(" + scale + "," + scale + ")";
        $(browserPrefixes).each(function () {
            element
                .css(this + "transform", scaleString);
        });
        element
            .data({ scale: scale })
            .addClass("scaled");
    };

    var scaleSlides = function() {
        $(".slides")
            .css("display", "block")
            .each(function () {
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

    $(window).load(function () {
        scaleSlides();
    });
    
    // Sortable slides.
    $(function () {
        $(".slides-wrapper.interactive").each(function() {
            var wrapper = $(this);

            var showChangedMessage = function () {
                wrapper.find(".dirty-message").show();
            };

            var onSlideIndexChanged = function (e, ui) {
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
(function ($) {
    $(function() {
        $(".engine-picker").on("change", function(e) {
            var engine = $(this).val();

            $(".engine-settings-editor").hide();
            $("[data-engine='" + engine + "']").show();
        }).trigger("change");
    });
})(jQuery);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImFkbWluLXNsaWRlcy1wYXJ0LmpzIiwiYWRtaW4tc2xpZGVzaG93LXBhcnQuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzVFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsImZpbGUiOiJBZG1pbi5qcyIsInNvdXJjZXNDb250ZW50IjpbIihmdW5jdGlvbiAoJCkge1xyXG4gICAgLy8gU2xpZGVzIHByZXZpZXcuXHJcbiAgICB2YXIgYXBwbHlDc3NTY2FsZSA9IGZ1bmN0aW9uIChlbGVtZW50LCBzY2FsZSwgdHJhbnNsYXRlKSB7XHJcbiAgICAgICAgdmFyIGJyb3dzZXJQcmVmaXhlcyA9IFtcIlwiLCBcIi1tcy1cIiwgXCItbW96LVwiLCBcIi13ZWJraXQtXCIsIFwiLW8tXCJdLFxyXG4gICAgICAgICAgICBvZmZzZXQgPSAoKDEgLSBzY2FsZSkgKiA1MCkgKyBcIiVcIixcclxuICAgICAgICAgICAgc2NhbGVTdHJpbmcgPSAodHJhbnNsYXRlICE9PSBmYWxzZSA/IFwidHJhbnNsYXRlKC1cIiArIG9mZnNldCArIFwiLCAtXCIgKyBvZmZzZXQgKyBcIikgXCIgOiBcIlwiKSArIFwic2NhbGUoXCIgKyBzY2FsZSArIFwiLFwiICsgc2NhbGUgKyBcIilcIjtcclxuICAgICAgICAkKGJyb3dzZXJQcmVmaXhlcykuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGVsZW1lbnRcclxuICAgICAgICAgICAgICAgIC5jc3ModGhpcyArIFwidHJhbnNmb3JtXCIsIHNjYWxlU3RyaW5nKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBlbGVtZW50XHJcbiAgICAgICAgICAgIC5kYXRhKHsgc2NhbGU6IHNjYWxlIH0pXHJcbiAgICAgICAgICAgIC5hZGRDbGFzcyhcInNjYWxlZFwiKTtcclxuICAgIH07XHJcblxyXG4gICAgdmFyIHNjYWxlU2xpZGVzID0gZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgJChcIi5zbGlkZXNcIilcclxuICAgICAgICAgICAgLmNzcyhcImRpc3BsYXlcIiwgXCJibG9ja1wiKVxyXG4gICAgICAgICAgICAuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgc2xpZGVzaG93ID0gJCh0aGlzKSxcclxuICAgICAgICAgICAgICAgICAgICBzbGlkZSA9IHNsaWRlc2hvdy5maW5kKFwiLnNsaWRlLXByZXZpZXdcIiksXHJcbiAgICAgICAgICAgICAgICAgICAgcGFyZW50ID0gc2xpZGUucGFyZW50KCksXHJcbiAgICAgICAgICAgICAgICAgICAgd2lkdGggPSAxNTAsXHJcbiAgICAgICAgICAgICAgICAgICAgaGVpZ2h0ID0gMTUwLFxyXG4gICAgICAgICAgICAgICAgICAgIGJvdW5kaW5nRGltZW5zaW9uID0gMTUwLFxyXG4gICAgICAgICAgICAgICAgICAgIHNsaWRlU3R5bGUgPSBzbGlkZS5hdHRyKFwic3R5bGVcIik7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKChzbGlkZVN0eWxlICE9IG51bGwgJiYgc2xpZGVTdHlsZS5pbmRleE9mKFwid2lkdGg6XCIpID09IC0xKSkgd2lkdGggPSAxMDI0O1xyXG4gICAgICAgICAgICAgICAgaWYgKChzbGlkZVN0eWxlICE9IG51bGwgJiYgc2xpZGVTdHlsZS5pbmRleE9mKFwiaGVpZ2h0OlwiKSA9PSAtMSkpIGhlaWdodCA9IDc2ODtcclxuXHJcbiAgICAgICAgICAgICAgICBzbGlkZS5jc3Moe1xyXG4gICAgICAgICAgICAgICAgICAgIHdpZHRoOiB3aWR0aCArIFwicHhcIixcclxuICAgICAgICAgICAgICAgICAgICBoZWlnaHQ6IGhlaWdodCArIFwicHhcIixcclxuICAgICAgICAgICAgICAgICAgICBwb3NpdGlvbjogXCJhYnNvbHV0ZVwiXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIHZhciBzY2FsZWRGb3JXaWR0aCA9IHdpZHRoID4gaGVpZ2h0LFxyXG4gICAgICAgICAgICAgICAgICAgIGxhcmdlc3REaW1lbnNpb24gPSAoc2NhbGVkRm9yV2lkdGggPyB3aWR0aCA6IGhlaWdodCksXHJcbiAgICAgICAgICAgICAgICAgICAgc2NhbGUgPSBib3VuZGluZ0RpbWVuc2lvbiAvIGxhcmdlc3REaW1lbnNpb247XHJcblxyXG4gICAgICAgICAgICAgICAgcGFyZW50LmNzcyh7XHJcbiAgICAgICAgICAgICAgICAgICAgd2lkdGg6IE1hdGguZmxvb3Iod2lkdGggKiBzY2FsZSkgKyBcInB4XCIsXHJcbiAgICAgICAgICAgICAgICAgICAgaGVpZ2h0OiBNYXRoLmZsb29yKGhlaWdodCAqIHNjYWxlKSArIFwicHhcIixcclxuICAgICAgICAgICAgICAgICAgICBwb3NpdGlvbjogXCJyZWxhdGl2ZVwiLFxyXG4gICAgICAgICAgICAgICAgICAgIG92ZXJmbG93OiBcImhpZGRlblwiXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgICAgICBhcHBseUNzc1NjYWxlKHNsaWRlLCBzY2FsZSk7XHJcbiAgICAgICAgICAgICAgICBzbGlkZXNob3cucGFyZW50KFwiLnNsaWRlcy13cmFwcGVyXCIpLmNzcyhcIm92ZXJmbG93XCIsIFwidmlzaWJsZVwiKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICB9O1xyXG5cclxuICAgICQod2luZG93KS5sb2FkKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICBzY2FsZVNsaWRlcygpO1xyXG4gICAgfSk7XHJcbiAgICBcclxuICAgIC8vIFNvcnRhYmxlIHNsaWRlcy5cclxuICAgICQoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICQoXCIuc2xpZGVzLXdyYXBwZXIuaW50ZXJhY3RpdmVcIikuZWFjaChmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgdmFyIHdyYXBwZXIgPSAkKHRoaXMpO1xyXG5cclxuICAgICAgICAgICAgdmFyIHNob3dDaGFuZ2VkTWVzc2FnZSA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHdyYXBwZXIuZmluZChcIi5kaXJ0eS1tZXNzYWdlXCIpLnNob3coKTtcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIHZhciBvblNsaWRlSW5kZXhDaGFuZ2VkID0gZnVuY3Rpb24gKGUsIHVpKSB7XHJcbiAgICAgICAgICAgICAgICBzaG93Q2hhbmdlZE1lc3NhZ2UoKTtcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIHZhciBzbGlkZXNMaXN0ID0gd3JhcHBlci5maW5kKFwidWwuc2xpZGVzXCIpO1xyXG4gICAgICAgICAgICBzbGlkZXNMaXN0LnNvcnRhYmxlKHtcclxuICAgICAgICAgICAgICAgIHBsYWNlaG9sZGVyOiBcInNvcnRhYmxlLXBsYWNlaG9sZGVyXCIsXHJcbiAgICAgICAgICAgICAgICBzdG9wOiBvblNsaWRlSW5kZXhDaGFuZ2VkXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBzbGlkZXNMaXN0LmRpc2FibGVTZWxlY3Rpb24oKTtcclxuICAgICAgICB9KTtcclxuICAgIH0pO1xyXG59KShqUXVlcnkpOyIsIihmdW5jdGlvbiAoJCkge1xyXG4gICAgJChmdW5jdGlvbigpIHtcclxuICAgICAgICAkKFwiLmVuZ2luZS1waWNrZXJcIikub24oXCJjaGFuZ2VcIiwgZnVuY3Rpb24oZSkge1xyXG4gICAgICAgICAgICB2YXIgZW5naW5lID0gJCh0aGlzKS52YWwoKTtcclxuXHJcbiAgICAgICAgICAgICQoXCIuZW5naW5lLXNldHRpbmdzLWVkaXRvclwiKS5oaWRlKCk7XHJcbiAgICAgICAgICAgICQoXCJbZGF0YS1lbmdpbmU9J1wiICsgZW5naW5lICsgXCInXVwiKS5zaG93KCk7XHJcbiAgICAgICAgfSkudHJpZ2dlcihcImNoYW5nZVwiKTtcclxuICAgIH0pO1xyXG59KShqUXVlcnkpOyJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==