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

    var disableContentEditor = function () {
        $(".content-disabled input").prop("disabled", true);
        $(".content-disabled textarea").prop("disabled", true);
        $(".content-disabled button").prop("disabled", true);
    };

    $(function() {
        initExpandoControl();
        disableContentEditor();
    });

})(jQuery);