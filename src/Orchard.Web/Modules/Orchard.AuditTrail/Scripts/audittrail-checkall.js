$(function () {
    $(".check-all-container").each(function () {
        var container = $(this);
        var master = container.find("input[type=\"checkbox\"].check-all-master");
        var slaves = container.find("input[type=\"checkbox\"]:not(:disabled).check-all-slave");

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