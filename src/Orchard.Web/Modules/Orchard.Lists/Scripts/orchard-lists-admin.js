jQuery("#publishActions").bind("change", function () {
    var value = jQuery(this).val(),
        target = jQuery("#TargetContainerId");
    if (value === "MoveToList") {
        target.css("display", "inline");
    }
    else {
        target.css("display", "none");
    }
});