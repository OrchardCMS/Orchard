(function($) {
    $(function() {
        $("#layout-content").on("click", ".submit-form", function (e) {
            $(this).parents("form:first").submit();
            e.preventDefault();
        });
    });
})(jQuery);