jQuery.fn.extend({
    toggleWhatYouControl: function() {
        var _controller = $(this);
        var _controllees = $("[data-controllerid=" + _controller.attr("id") + "]");
        if (_controller.is(":checked")) {
            $(_controllees.slideDown(200)[0]).find("input").focus();
        } else {
            _controllees.slideUp(200);
        }
    }
});
(function() {
    $("[data-controllerid]").each(function() {
        var controller = $("#" + $(this).attr("data-controllerid"));
        if (!controller.is(":checked")) {
            $("[data-controllerid=" + controller.attr("id") + "]").hide();
        }
        if (controller.is(":checkbox")) {
            controller.click($(this).toggleWhatYouControl);
        } else if (controller.is(":radio")) {
            $("[name=" + controller.attr("name") + "]").click(function() { $("[name=" + $(this).attr("name") + "]").each($(this).toggleWhatYouControl); });
        }
    });
})();