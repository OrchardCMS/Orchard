(function($) {
    $(function() {
        $("table.items").each(function() {
            var table = $(this);

            table.on("click", ".check-schema, .check-data", function (e) {
                updateState();
            });

            table.on("click", ".check-both", function (e) {
                var sender = $(this);
                var isChecked = sender.is(":checked");
                var row = sender.closest("tr");

                row.find(".check-schema, .check-data").prop("checked", isChecked);
                updateState();
            });

            table.on("click", ".check-all-schema", function (e) {
                var sender = $(this);
                var isChecked = sender.is(":checked");

                table.find(".check-schema").prop("checked", isChecked);
                updateState();
            });

            table.on("click", ".check-all-data", function (e) {
                var sender = $(this);
                var isChecked = sender.is(":checked");

                table.find(".check-data").prop("checked", isChecked);
                updateState();
            });

            table.on("click", ".check-all-both", function (e) {
                var sender = $(this);
                var isChecked = sender.is(":checked");

                table.find(".check-schema").prop("checked", isChecked);
                table.find(".check-data").prop("checked", isChecked);
                updateState();
            });

            var updateState = function () {
                table.find("tbody tr").each(function() {
                    var tr = $(this);
                    var checkSchema = tr.find(".check-schema").is(":checked");
                    var checkData = tr.find(".check-data").is(":checked");
                    
                    tr.find(".check-both").prop("checked", checkSchema && checkData);
                });

                var allBothChecked = table.find(".check-both").not(":checked").length === 0;
                var allSchemaChecked = table.find(".check-schema").not(":checked").length === 0;
                var allDataChecked = table.find(".check-data").not(":checked").length === 0;

                table.find(".check-all-both").prop("checked", allBothChecked);
                table.find(".check-all-schema").prop("checked", allSchemaChecked);
                table.find(".check-all-data").prop("checked", allDataChecked);
            };
        });
    });
})(jQuery);