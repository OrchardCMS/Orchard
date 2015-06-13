/// <reference path="typings/jquery/jquery.d.ts"/>
/// <reference path="typings/jquery/jqueryui.d.ts"/>
/// <reference path="typings/the-admin.d.ts"/>
var WidgetsContainer;
(function (WidgetsContainer) {
    $(function () {
        var removedWidgets = [];
        // Handle Add Widget button.
        $(".add-widget").on("click", function (e) {
            e.preventDefault();
            var hostId = $(this).data("host-id");
            var form = $(this).parents("form:first");
            var fieldset = $(this).parents("fieldset:first");
            var formActionValue = fieldset.find("input[name='submit.Save']");
            var url = $(this).attr("href");
            if (hostId === 0) {
                form.attr("action", url);
            }
            else {
                formActionValue.val("submit.Save");
                $("input[type='hidden'][name='returnUrl']").val(url);
            }
            form.submit();
        });
        // Handle Delete Widget button.
        $("div.widgets").on("click", "a.remove-widget", function (e) {
            e.preventDefault();
            if (!confirm($(this).data("confirm")))
                return;
            var li = $(this).parents("li:first");
            var widgetId = li.data("widget-id");
            li.remove();
            removedWidgets.push(widgetId);
            $("input[name='removedWidgets']").val(JSON.stringify(removedWidgets));
            updateWidgetPlacementField();
        });
        var updateWidgetPlacementField = function () {
            var widgetPlacementField = $("input[name='widgetPlacement']");
            var data = {
                zones: {}
            };
            $("div.widgets ul.widgets").each(function () {
                var zone = $(this).data("zone");
                data.zones[zone] = {
                    widgets: []
                };
                $(this).find("li").each(function () {
                    var widgetId = $(this).data("widget-id");
                    data.zones[zone].widgets.push(widgetId);
                });
            });
            var text = JSON.stringify(data);
            widgetPlacementField.val(text);
        };
        // Initialize sortable widgets.
        $("div.widgets ul.widgets").sortable({
            connectWith: "div.widgets ul.widgets",
            dropOnEmpty: true,
            placeholder: "sortable-placeholder",
            receive: function (e, ui) {
                updateWidgetPlacementField();
            },
            update: function (e, ui) {
                updateWidgetPlacementField();
            }
        });
        // Initialize Expando control
        $("#widgetsPlacement legend").expandoControl(function (controller) {
            return controller.nextAll(".expando");
        }, {
            collapse: true,
            remember: true
        });
    });
})(WidgetsContainer || (WidgetsContainer = {}));
//# sourceMappingURL=admin.widgets-container.js.map