(function ($) {
    $.fn.extend({
        powerUpTheSwitch: function () {
            var _this = $(this);
            var theSwitch = $("<div class=\"switch-for-switchable\"><ul class=\"switch-button-group\"><li class=\"switch-button summary-view\">&nbsp;</li><li class=\"switch-button detail-view\"></li></ul></div>");

            var summarySwitch = theSwitch.find(".summary-view").click(function () { $(this).switchToSummaryView(_this); });
            var detailSwitch = theSwitch.find(".detail-view").click(function () { $(this).switchToDetailView(_this); });

            var setting = $.orchard.setting("switchable", { path: document.location.pathname })
                || (_this.hasClass("summary-view") ? "summary-view" : "detail-view");
            if (setting === "summary-view") {
                summarySwitch.switchToSummaryView(_this);
            } else {
                detailSwitch.switchToDetailView(_this);
            }

            theSwitch.insertBefore(_this);
        },
        switchToDetailView: function (switched) {
            $.orchard.setting("switchable", "detail-view", { path: document.location.pathname });
            $(this).closest(".switch-for-switchable").addClass("detail-switched").removeClass("summary-switched");
            switched.addClass("detail-view").removeClass("summary-view");
        },
        switchToSummaryView: function (switched) {
            $.orchard.setting("switchable", "summary-view", { path: document.location.pathname });
            $(this).closest(".switch-for-switchable").addClass("summary-switched").removeClass("detail-switched");
            switched.addClass("summary-view").removeClass("detail-view");
        }
    });
    $(".switchable").powerUpTheSwitch();
})(jQuery);