(function($) {
    $(function() {
        $("#layout-content").on("click", "#button-close", function (e) {
            parent.$.colorbox.close();
            e.preventDefault();
        });
    });
})(jQuery);