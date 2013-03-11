(function($) {
    $(function() {
        $("#search-box").focus().on("keyup", function() {
            var text = $(this).val().toLowerCase();
            
            if (text == "") {
                $("[data-record-text]").show();
            } else {
                $("[data-record-text]").each(function() {
                    var recordText = $(this).data("record-text").toLowerCase();
                    $(this).toggle(recordText.indexOf(text) >= 0);
                });
            }
        });
    });
})(jQuery);