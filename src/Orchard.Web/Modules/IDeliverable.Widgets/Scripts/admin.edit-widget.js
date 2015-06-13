/// <reference path="typings/jquery/jquery.d.ts"/>
var WidgetsContainer;
(function (WidgetsContainer) {
    $(function () {
        var widgetPartLayerId = $("#WidgetPart_LayerId");
        var fieldset = widgetPartLayerId.parents("fieldset:first");
        fieldset.hide();
    });
})(WidgetsContainer || (WidgetsContainer = {}));
//# sourceMappingURL=admin.edit-widget.js.map