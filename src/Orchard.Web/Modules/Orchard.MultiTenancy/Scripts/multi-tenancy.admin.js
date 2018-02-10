(function ($) {
    $(function () {
        $(".available-extensions")
            .find("label")
            .expandoControl(function (controller) { return controller.next("div"); }, { collapse: false, remember: true });

        $(".select-all").click(function () {
            var $checkbox = $(this);
            var $allCheckboxes = $checkbox.closest("ol").find(":checkbox");
            if ($checkbox.is(':checked')) {
                $allCheckboxes.prop("checked", true);
            }
            else {
                $allCheckboxes.prop("checked", false);
            }
        });

        $('input[name=DataProvider]:checked').click();
    });
})(jQuery);