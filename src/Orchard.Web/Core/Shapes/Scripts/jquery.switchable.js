(function ($) {
    $.fn.extend({
        powerUpTheSwitch: function () {
            var _this = $(this);
            var theSwitch = $("<div class=\"text-right clear switch-for-switchable\" data-toggle=\"buttons\"><label class=\"btn btn-default active summary-view\"><input type=\"radio\" name=\"options\" id=\"option1\" autocomplete=\"off\" checked><i class=\"fa fa-th\"></i></label><label class=\"btn btn-default detail-view\"><input type=\"radio\" name=\"options\" id=\"option2\" autocomplete=\"off\"><i class=\"fa fa-list\"></i></label></div>");
            
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