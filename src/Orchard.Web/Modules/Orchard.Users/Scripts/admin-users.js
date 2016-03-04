﻿(function ($) {
    $(function () {
        $.extend(true, $.fn.dataTable.defaults, {
            dom:
                "<'row am-datatable-header'<'col-sm-6'><'col-sm-6 pull-right'>>" +
                "<'row am-datatable-body'<'col-sm-12'tr>>" +
                "<'row am-datatable-footer'<'col-sm-5'><'col-sm-7'>>"
        });

        // Initialize the data tables.
        var dataTableOptions = {
            pageLength: 50,
            "columnDefs": [{
                "targets": 'no-sort',
                "orderable": false,
                "order": []
            }]
        };

        var dataTable = $(".data-table").DataTable(dataTableOptions);

        $("#search-box").on("keyup", function (e) {
            var text = $(this).val();
            
            // Filter.
            dataTable.search(text).draw();

            // Check for Enter key.
            if (e.keyCode === 13) {
                var visibleRows = dataTable.rows( { search:"applied"} );
                var rowCount = visibleRows.count();

                // No rows.
                if (rowCount === 0) {
                    var primaryButton = $(".create-new");
                    location.href = primaryButton.attr("href") + "?suggestion=" + text;
                    return;
                }

                // At least one row.
                var firstRow = $(visibleRows.row({ search: "applied" }).node());
                var editLink = firstRow.find("td a.edit-link");
                var href = editLink.attr("href");
                location.href = href;
            }
        });
        
    });

})(jQuery);