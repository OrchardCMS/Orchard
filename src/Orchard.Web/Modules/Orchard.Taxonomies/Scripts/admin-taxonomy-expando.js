(function ($) {
    $("fieldset legend").expandoControl(function (controller) { return controller.nextAll(".expando"); }, { collapse: true, remember: false });
})(jQuery);