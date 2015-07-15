$(function() {

    var initializeFeaturesUI = function() {
        var bulkActions = $(".bulk-actions-wrapper").addClass("visible");
        var theSwitch = $(".switch-for-switchable");
        theSwitch.prepend(bulkActions);
        $("#search-box").focus().keyup(function() {
            var text = $(this).val();

            if (text == '') {
                $("li.category").show();
                $(".feature-item:hidden").show();
                return;
            }

            $(".feature-item").each(function () {
                var elt = $(this);
                var value = elt.find('h4:first').text();
                if (value.toLowerCase().indexOf(text.toLowerCase()) >= 0)
                    elt.show();
                else
                    elt.hide();
            });

            $("li.category:hidden").show();
            var toHide = $("li.category:not(:has(.feature-item:visible))").hide();
        });
    };

    var initializeSelectionBehavior = function() {
        $("li.feature h4").on("change", "input[type='checkbox']", function() {
            var checked = $(this).is(":checked");
            var wrapper = $(this).parents("li.feature:first");
            wrapper.toggleClass("selected", checked);
        });
    };

    var initializeActionLinks = function() {
        $("li.feature-item .actions").on("click", "button[data-feature-action]", function(e) {
            var actionLink = $(this);
            var featureId = actionLink.data("feature-id");
            var action = actionLink.data("feature-action");
            var force = actionLink.data("feature-force");
            var dependants = actionLink.data("feature-dependants");

            if (!dependants || /^\s*$/.test(dependants) || confirm(confirmDisableMessage + "\n\n" + dependants)) {

                $("[name='submit.BulkExecute']").val("yes");
                $("[name='featureIds']").val(featureId);
                $("[name='bulkAction']").val(action);
                $("[name='force']").val(force);

                actionLink.parents("form:first").submit();
            }

            e.preventDefault();
        });
    };

    initializeFeaturesUI();
    initializeSelectionBehavior();
    initializeActionLinks();
});