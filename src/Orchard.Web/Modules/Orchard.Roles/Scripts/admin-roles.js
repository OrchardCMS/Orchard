(function ($) {
    $(function () {

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