(function ($) {
    $.fn.extend({
        powerUpTheSwitch: function () {
            var _this = $(this);

            var setting = $.orchard.setting("switchable", { path: document.location.pathname })
                || (_this.hasClass("summary-view") ? "summary-view" : "detail-view");

            var theSwitch = $("<div class=\"text-right clear switch-for-switchable\" data-toggle=\"buttons\"><label class=\"btn btn-default " +
                (setting == "summary-view" ? "active" : "") +
                " summary-view\"><input type=\"radio\" name=\"options\" id=\"option1\" autocomplete=\"off\" " +
                (setting == "summary-view" ? "checked" : "") +
                "><i class=\"fa fa-th\"></i></label><label class=\"btn btn-default " +
                (setting == "detail-view" ? "active" : "") +
                " detail-view\"><input type=\"radio\" name=\"options\" id=\"option2\" autocomplete=\"off\" " +
                (setting == "detail-view" ? "checked" : "") +
                "><i class=\"fa fa-list\"></i></label></div>");
            
            var summarySwitch = theSwitch.find(".summary-view").click(function () { $(this).switchToSummaryView(_this); });
            var detailSwitch = theSwitch.find(".detail-view").click(function () { $(this).switchToDetailView(_this); });

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