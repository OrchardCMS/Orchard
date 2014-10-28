$(function () {

    var executeSelectedAction = function(e, form) {
        var actionList = form.find("[name=\"RecycleBinCommand\"]");
        var selectedAction = actionList.find("option:selected");
        var prompt = selectedAction.data("unsafe-action");

        if (prompt) {
            if (!confirm(prompt)) {
                e.preventDefault();
                return;
            }
        }
    };

    $("#recycle-bin").on("submit", "form", function(e) {
        var form = $(this);
        var executeActionButton = form.find("[name=\"ExecuteActionButton\"]");

        if (executeActionButton.val() == "ExecuteActionButton") {
            executeSelectedAction(e, form);
        }
    });
});