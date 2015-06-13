/// <reference path="typings/jquery/jquery.d.ts"/>
/// <reference path="typings/jquery/jqueryui.d.ts"/>
/// <reference path="typings/the-admin.d.ts"/>

module WidgetsContainer {
    $(function () {

        var removedWidgets = [];

        // Handle Add Widget button.
        $(".add-widget").on("click", function (e: JQueryEventObject) {
            e.preventDefault();
            var hostId: number = $(this).data("host-id");
            var form: JQuery = $(this).parents("form:first");
            var fieldset: JQuery = $(this).parents("fieldset:first");
            var formActionValue: JQuery = fieldset.find("input[name='submit.Save']");
            var url: string = $(this).attr("href");

            if(hostId === 0){
                form.attr("action", url);
            }
            else{
                formActionValue.val("submit.Save");
                $("input[type='hidden'][name='returnUrl']").val(url);
            }
            
            form.submit();
        });

        // Handle Delete Widget button.
        $("div.widgets").on("click", "a.remove-widget", function (e: JQueryEventObject) {
            e.preventDefault();

            if(!confirm($(this).data("confirm")))
                return;

            var li: JQuery = $(this).parents("li:first");
            var widgetId: number = li.data("widget-id");

            li.remove();
            removedWidgets.push(widgetId);
            $("input[name='removedWidgets']").val(JSON.stringify(removedWidgets));
            updateWidgetPlacementField();
        });

        var updateWidgetPlacementField = function () {
            var widgetPlacementField: JQuery = $("input[name='widgetPlacement']");
            var data = {
                zones: {}
            };
            $("div.widgets ul.widgets").each(function(){
                var zone: string = $(this).data("zone");
                
                data.zones[zone] = {
                    widgets: []
                };

                $(this).find("li").each(function(){
                    var widgetId: number = $(this).data("widget-id");
                    data.zones[zone].widgets.push(widgetId);
                });
            });

            var text: string = JSON.stringify(data);
            widgetPlacementField.val(text);
        };

        // Initialize sortable widgets.
        $("div.widgets ul.widgets").sortable({
            connectWith: "div.widgets ul.widgets",
            dropOnEmpty: true,
            placeholder: "sortable-placeholder",
            receive: function(e, ui){
                updateWidgetPlacementField();
            },
            update: function(e, ui){
                updateWidgetPlacementField();
            }
        });

        // Initialize Expando control
        $("#widgetsPlacement legend").expandoControl(
            function (controller) { 
                return controller.nextAll(".expando"); }, { 
                    collapse: true, 
                    remember: true });
    });
}