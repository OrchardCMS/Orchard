(function($) {
    // Initialize Expando control
    $(".expando-wrapper legend").expandoControl(
        function (controller) {
            return controller.nextAll(".expando");
        }, {
            collapse: true,
            remember: true
        });
})(jQuery);