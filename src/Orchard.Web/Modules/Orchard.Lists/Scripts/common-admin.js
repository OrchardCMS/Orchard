(function($) {
    $(function() {
        $("#layout-main").on("click", "#button-close", function (e) {
            parent.$.colorbox.close();
            e.preventDefault();
        });
    });
})(jQuery);