$(function () {

    var executeSelectedAction = function(e, form) {
        var actionList = form.find("[name=\"RecycleBinCommand\"]");
        var selectedAction = actionList.find("option:selected");
        var prompts = selectedAction.data("unsafe-action");

        if (prompts) {
            if (!Array.isArray(prompts))
                prompts = [prompts];

            for (var i = 0; i < prompts.length; i++) {
                if (!confirm(prompts[i])) {
                    e.preventDefault();
                    return;
                }
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