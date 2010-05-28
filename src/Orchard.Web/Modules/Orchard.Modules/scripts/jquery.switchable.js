(function ($) {
    $.fn.extend({
        powerUpTheSwitch: function () {
            var _this = $(this);
            var theSwitch = $("<div class=\"switch-for-switchable\"><ul class=\"switch-button-group\"><li class=\"switch-button summary-view\">&nbsp;</li><li class=\"switch-button detail-view\"></li></ul></div>");

            theSwitch.find(".summary-view").click(function () { $(this).switchToSummaryView(_this); });
            theSwitch.find(".detail-view").click(function () { $(this).switchToDetailView(_this); });

            theSwitch.addClass(_this.hasClass("summary-view") ? "summary-switched" : "detail-switched");

            theSwitch.insertBefore(_this);
        },
        switchToDetailView: function (switched) {
            $(this).closest(".switch-for-switchable").addClass("detail-switched").removeClass("summary-switched");
            switched.addClass("detail-view").removeClass("summary-view");
        },
        switchToSummaryView: function (switched) {
            $(this).closest(".switch-for-switchable").addClass("summary-switched").removeClass("detail-switched");
            switched.addClass("summary-view").removeClass("detail-view");
        }
    });
    $(".switchable").powerUpTheSwitch();
})(jQuery);