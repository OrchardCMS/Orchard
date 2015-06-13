/// <reference path="typings/jquery/jquery.d.ts"/>

module WidgetsContainer {
    $(function () {
        var widgetPartLayerId = $("#WidgetPart_LayerId");
        var fieldset = widgetPartLayerId.parents("fieldset:first");
        fieldset.hide();
    });
}