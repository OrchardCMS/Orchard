(function ($) {
    $(function() {
        $(".media-library-picker-field").on("opened", function(e) {
            window.parent.currentDialog.toggleCommands(false);
        });

        $(".media-library-picker-field").on("closed", function(e) {
            window.parent.currentDialog.toggleCommands(true);
        });
    });
})(jQuery);