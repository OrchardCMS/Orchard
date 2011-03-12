(function ($) {

    function sync() {
        var value = $(this).val(),
            target = $("#TargetContainerId");
        if (value === "MoveToList") {
            target.css("display", "inline");
        }
        else {
            target.css("display", "none");
        }
    }

    $("#publishActions").bind("change", sync).each(sync);

})(jQuery);
