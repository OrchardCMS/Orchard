(function($) {

    var initExpandoControl = function() {
        $(".expando-wrapper legend").expandoControl(
            function(controller) {
                return controller.nextAll(".expando");
            }, {
                collapse: true,
                remember: true
            });
    };

    var initCheckAll = function() {
        $("table.check-all").each(function() {
            var table = $(this);
            var controller = table.find("thead input[type=\"checkbox\"]");
            var checkboxes = table.find("tbody input[type=\"checkbox\"]");

            var updateController = function () {
                var allChecked = checkboxes.filter(":not(:checked)").length == 0;
                controller.prop("checked", allChecked);
            }

            table.on("change", "thead input[type=\"checkbox\"]", function() {
                var isChecked = $(this).is(":checked");
                checkboxes.prop("checked", isChecked);
            });

            table.on("change", "tbody input[type=\"checkbox\"]", function () {
                updateController();
            });

            updateController();
        });
    };

    var disableContentEditor = function () {
        $(".content-disabled input").prop("disabled", true);
        $(".content-disabled textarea").prop("disabled", true);
        $(".content-disabled button").prop("disabled", true);
    };

    $(function() {
        initExpandoControl();
        initCheckAll();
        disableContentEditor();
    });

})(jQuery);