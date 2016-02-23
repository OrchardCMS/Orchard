(function($) {
    $(function () {
        $("table.items thead tr input[type='checkbox'].check-all-in-column").each(function () {
            var master = $(this);
            var columnIndex = master.closest("th").index() + 1;
            var slaves = master.closest("table").find("tbody tr td:nth-child(" + columnIndex + ") input[type='checkbox']:not(:disabled)");

            var updateMaster = function () {
                var allChecked = slaves.filter(":not(:checked)").length == 0;
                master.prop("checked", allChecked);
            }

            master.on("change", function () {
                var isChecked = $(this).is(":checked");
                slaves.prop("checked", isChecked);
            });

            slaves.on("change", function () {
                updateMaster();
            });

            updateMaster();
        });
    });
})(jQuery);