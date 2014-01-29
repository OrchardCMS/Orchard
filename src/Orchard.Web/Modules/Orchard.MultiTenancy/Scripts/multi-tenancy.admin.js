(function ($) {
    $(function () {
        $(".select-all").click(function () {
            var $checkbox = $(this);
            var $allCheckboxes = $checkbox.parent().find(":checkbox");
            if ($checkbox.is(':checked')) {
                $allCheckboxes.prop("checked", true);
            }
            else {
                $allCheckboxes.prop("checked", false);
            }
        });
    });
})(jQuery);