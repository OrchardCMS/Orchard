jQuery.fn.extend({
    helpfullyFocus: function() {
        var _this = $(this);
        var firstError = _this.find(".input-validation-error").first();
        return firstError.size() === 1
            ? firstError.focus()
            : _this.find("input:text").first().focus();
    },
    toggleWhatYouControl: function() {
        var _this = $(this);
        var _controllees = $("[data-controllerid=" + _this.attr("id") + "]");
        if (_this.is(":checked")) {
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