(function ($) {

    var initializePagers = function () {
        $('select.pager.selector').change(function() {
            window.location = $(this).attr("disabled", true).val();
        });
    };

    $(document).ready(function () {
        App.init();

        initializePagers();
    });
})(jQuery);