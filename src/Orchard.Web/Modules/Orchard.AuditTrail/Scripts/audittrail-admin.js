(function($) {

    var initExpandoControl = function() {
        $(".expando-wrapper legend").expandoControl(
            function(controller) {
                return controller.nextAll(".expando");
            }, {
                collapse: true,
                remember: true
            });
    };

    $(function() {
        initExpandoControl();
    });

})(jQuery);